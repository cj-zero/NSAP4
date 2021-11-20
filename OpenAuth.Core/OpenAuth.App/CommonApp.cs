   using Infrastructure;
using Infrastructure.Cache;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Settlement;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App
{
    public class CommonApp : OnlyUnitWorkBaeApp
    {
        private readonly ICacheContext _cacheContext;
        private readonly List<StatusList> monthTemp = new List<StatusList>
        {
            new StatusList { ID = 1,Name = "1月" } ,
            new StatusList { ID = 2 ,Name = "2月"},
            new StatusList { ID = 3 ,Name = "3月"},
            new StatusList { ID = 4 ,Name = "4月"},
            new StatusList { ID = 5 ,Name = "5月"},
            new StatusList { ID = 6 ,Name = "6月"},
            new StatusList { ID = 7,Name = "7月" } ,
            new StatusList { ID = 8 ,Name = "8月"},
            new StatusList { ID = 9 ,Name = "9月"},
            new StatusList { ID = 10 ,Name = "10月"},
            new StatusList { ID = 11 ,Name = "11月"},
            new StatusList { ID = 12 ,Name = "12月"}
        };
        public CommonApp(IUnitWork unitWork, IAuth auth, ICacheContext cacheContext) : base(unitWork, auth)
        {
            _cacheContext = cacheContext;
        }

        #region 首页报表

        /// <summary>
        /// 获取首页四个数量汇总
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetTopNumCard()
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();
            //List<string> userId = new List<string>();
            //userId.Add(loginContext.User.Id);
            //if (loginContext.Roles.Any(c => c.Name == "呼叫中心" || c.Name == "总经理" || c.Name == "总助") || loginContext.User.Account == Define.SYSTEM_USERNAME)
            //{
            //    userId.Clear();
            //}
            //else if (loginContext.Roles.Any(c => c.Name == "部门主管"))
            //{
            //    var orgId = loginContext.Orgs.OrderByDescending(c => c.CascadeId).FirstOrDefault().Id;
            //    var userid = await UnitWork.Find<Relevance>(c => c.Key == Define.USERORG && c.SecondId == orgId).Select(c => c.FirstId).ToListAsync();
            //    userId.AddRange(userid);
            //}
            List<dynamic> reportResp = new List<dynamic>();
            var now = DateTime.Now;
            var startdate = now.AddDays(-14).Date;
            decimal weekCompare = 0, dayCompare = 0;

            #region 服务呼叫
            var serviceOrder = await UnitWork.Find<ServiceOrder>(c => c.CreateTime >= startdate && c.CreateTime <= DateTime.Now).Select(c => new { c.Id, c.CreateTime }).ToListAsync();
            CalculateCompare(ref weekCompare, ref dayCompare, serviceOrder.Select(c => c.CreateTime).ToList());//计算同比

            var serviceOrderIds = serviceOrder.Where(c => c.CreateTime >= now.Date && c.CreateTime <= now).Select(c => c.Id).ToList();
            var sow = await UnitWork.Find<ServiceWorkOrder>(c => serviceOrderIds.Contains(c.ServiceOrderId)).ToListAsync();

            var currentSowStatus = sow.GroupBy(c => c.Status).Select(c => new { c.Key, Count = c.Count() }).ToList();
            var sowStatusTemp = await UnitWork.Find<Category>(c => c.TypeId == "SYS_ServiceWorkOrderStatus" && c.SortNo > 0).Select(c => new { Id = int.Parse(c.DtValue), Name = c.Name }).ToListAsync();

            var sowStatus = from a in sowStatusTemp
                            join b in currentSowStatus on a.Id equals b.Key into ab
                            from b in ab.DefaultIfEmpty()
                            select new { ID = a.Id, Name = a.Name, Count = b == null ? 0 : b.Count };
            reportResp.Add(new { Quantity = serviceOrderIds.Count, Type = "ServiceOrder", WeekCompare = weekCompare, DayCompare = dayCompare, StatusList = sowStatus });
            #endregion

            #region 退料
            var retrunnote = await UnitWork.Find<ReturnNote>(c => c.CreateTime >= startdate && c.CreateTime <= DateTime.Now).ToListAsync();
            CalculateCompare(ref weekCompare, ref dayCompare, retrunnote.Select(c => c?.CreateTime).ToList());//计算同比
            var currentRN = retrunnote.Where(c => c.CreateTime >= now.Date && c.CreateTime <= now).Select(c => c.FlowInstanceId).ToList();
            var flowinstace = await UnitWork.Find<FlowInstance>(c => currentRN.Contains(c.Id)).ToListAsync();
            var currentRNStatus = flowinstace.GroupBy(c => c.ActivityName).Select(c => new { c.Key, Count = c.Count() }).ToList();
            List<dynamic> rnStatusTemp = new List<dynamic>
            {
                new  { Name = "储运收货" } ,
                new  { Name = "品质检验" },
                new  { Name = "总经理审批" },
                new  { Name = "仓库入库" }
            };
            var rnStaus = from a in rnStatusTemp
                          join b in currentRNStatus on a.Name equals b.Key into ab
                          from b in ab.DefaultIfEmpty()
                          select new { Name = a.Name, Count = b == null ? 0 : b.Count };
            reportResp.Add(new { Quantity = currentRN.Count, Type = "ReturnNote", WeekCompare = weekCompare, DayCompare = dayCompare, StatusList = rnStaus });
            #endregion

            #region 报销
            var reimburseinfo = await UnitWork.Find<ReimburseInfo>(c => c.CreateTime >= startdate && c.CreateTime <= DateTime.Now).Select(c => new { c.CreateTime, c.FlowInstanceId }).ToListAsync();
            CalculateCompare(ref weekCompare, ref dayCompare, reimburseinfo.Select(c => c?.CreateTime).ToList());//计算同比
            var currentRE = reimburseinfo.Where(c => c.CreateTime >= now.Date && c.CreateTime <= now).Select(c => c.FlowInstanceId).ToList();
            var flowinstaceRE = await UnitWork.Find<FlowInstance>(c => currentRE.Contains(c.Id)).ToListAsync();
            var currentREStatus = flowinstaceRE.GroupBy(c => c.ActivityName).Select(c => new { c.Key, Count = c.Count() }).ToList();
            List<dynamic> reStatusTemp = new List<dynamic>
            {
                new { ID = "客服主管审批",Name = "客服主管审批" } ,
                new { ID = "财务初审" ,Name = "财务初审"},
                new { ID = "财务复审" ,Name = "财务复审"},
                new { ID = "总经理审批" ,Name = "总经理审批"},
                new { ID = "出纳" ,Name = "出纳"},
                new { ID = "结束" ,Name = "已支付"}
            };

            var reStaus = from a in reStatusTemp
                          join b in currentREStatus on a.ID equals b.Key into ab
                          from b in ab.DefaultIfEmpty()
                          select new { Name = a.Name, Count = b == null ? 0 : b.Count };
            reportResp.Add(new { Quantity = currentRE.Count, Type = "ReimburseInfo", WeekCompare = weekCompare, DayCompare = dayCompare, StatusList = reStaus });
            #endregion

            #region 销售订单
            var quotationoperationhistory = await UnitWork.Find<QuotationOperationHistory>(c => c.CreateTime >= startdate && c.CreateTime <= DateTime.Now && c.Action == "客户确认报价单").ToListAsync();
            CalculateCompare(ref weekCompare, ref dayCompare, quotationoperationhistory.Select(c => c?.CreateTime).ToList());//计算同比
            var currentQT = quotationoperationhistory.Where(c => c.CreateTime >= now.Date && c.CreateTime <= now).ToList();

            var quotation = await UnitWork.Find<Quotation>(null).Select(c => c.QuotationStatus).ToListAsync();
            var currentQTStatus = quotation.GroupBy(c => c).Select(c => new { c.Key, Count = c.Count() }).ToList();

            List<dynamic> quotationStatusTemp = new List<dynamic> {
                new { ID = 3.1M, Name = "业务员审批" } ,
                new { ID = 4, Name = "工程审批" } ,
                new { ID = 5, Name = "总经理审批" } ,
                new { ID = 8, Name = "财务审批" } ,
                new { ID = 10, Name = "仓库审批" } ,
                new { ID = 12, Name = "仓库审批" } ,
                new { ID = 11, Name = "已出库" }
            };

            var qtStaus = from a in quotationStatusTemp
                          join b in currentQTStatus on a.ID equals b.Key into ab
                          from b in ab.DefaultIfEmpty()
                          select new { Name = a.Name, Count = b == null ? 0 : b.Count };
            reportResp.Add(new { Quantity = currentQT.Count, Type = "Quotation", WeekCompare = weekCompare, DayCompare = dayCompare, StatusList = qtStaus.GroupBy(c => c.Name).Select(c => new { Name = c.Key, Count = c.Sum(o => o.Count) }) });

            #endregion
            result.Data = reportResp;
            return result;
        }

        /// <summary>
        /// 获取首页图表数据
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetChartInfo()
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();
            //_cacheContext.Remove("HomePageChartInfo");
            var data = _cacheContext.Get<dynamic>("HomePageChartInfo");
            if (data != null)
            {
                result.Data = data;
            }
            else
            {
                var startdate = DateTime.Now.AddDays(-7).Date;
                var enddate = DateTime.Now;

                List<dynamic> reportResp = new List<dynamic>();
                List<dynamic> resultList = new List<dynamic>();
                List<string> dateTemp = new List<string>();
                for (int i = 7; i > 0; i--)
                {
                    dateTemp.Add(DateTime.Now.AddDays(-i).ToString("yyyy-MM-dd"));
                }
                //销售总额
                var quotationQuery = await (from a in UnitWork.Find<QuotationOperationHistory>(c => c.CreateTime >= startdate && c.CreateTime < enddate.Date && c.Action == "客户确认报价单")
                                            join b in UnitWork.Find<Quotation>(null) on a.QuotationId equals b.Id into ab
                                            from b in ab.DefaultIfEmpty()
                                            select new { a.CreateTime, b.TotalMoney }).ToListAsync();
                var quotationList = quotationQuery.GroupBy(c => c.CreateTime.Value.ToString("yyyy-MM-dd")).Select(c => new
                {
                    key = c.Key,
                    totalMoney = c.Sum(o => o.TotalMoney)
                });
                quotationList = from a in dateTemp
                                join b in quotationList on a equals b.key into ab
                                from b in ab.DefaultIfEmpty()
                                select new { key = a, totalMoney = b == null ? 0 : b.totalMoney };

                resultList.Add(new { type = "Quotation", resultList = quotationList });

                //报销总额
                var reimburseinfo = await UnitWork.Find<ReimburseInfo>(c => c.CreateTime >= startdate && c.CreateTime < enddate.Date).ToListAsync();
                var reimburseList = reimburseinfo.GroupBy(c => c.CreateTime.ToString("yyyy-MM-dd")).Select(c => new
                {
                    key = c.Key,
                    totalMoney = c.Sum(o => o.TotalMoney)
                });

                reimburseList = from a in dateTemp
                                join b in reimburseList on a equals b.key into ab
                                from b in ab.DefaultIfEmpty()
                                select new { key = a, totalMoney = b == null ? 0 : b.totalMoney };

                resultList.Add(new { type = "ReimburseInfo", resultList = reimburseList });
                reportResp.Add(new { type = "SaleAndReimburse", resultList = resultList });

                //退料汇总
                var retrunnote = await UnitWork.Find<ReturnNote>(c => c.CreateTime >= startdate && c.CreateTime < enddate.Date).ToListAsync();
                var retrunnoteList = retrunnote.GroupBy(c => c.CreateTime.ToString("yyyy-MM-dd")).Select(c => new
                {
                    key = c.Key,
                    totalMoney = c.Sum(o => o.TotalMoney)
                });

                retrunnoteList = from a in dateTemp
                                 join b in retrunnoteList on a equals b.key into ab
                                 from b in ab.DefaultIfEmpty()
                                 select new { key = a, totalMoney = b == null ? 0 : b.totalMoney };

                reportResp.Add(new { type = "ReturnNote", resultList = retrunnoteList });

                //服务检测
                var category = await UnitWork.Find<Category>(c => c.TypeId == "SYS_ServiceWorkOrderStatus" && c.SortNo > 0).ToListAsync();
                var serviceorder = await UnitWork.Find<ServiceWorkOrder>(null).Select(c => new { c.Status, c.CreateTime }).ToListAsync();
                var histogram = serviceorder.Where(c => c.CreateTime >= startdate && c.CreateTime < enddate.Date).GroupBy(c => c.CreateTime.Value.ToString("yyyy-MM-dd")).Select(c => new
                {
                    key = c.Key,
                    pending = c.Where(c => c.Status < 7).Count(),
                    processed = c.Where(c => c.Status >= 7).Count()
                });

                histogram = from a in dateTemp
                            join b in histogram on a equals b.key into ab
                            from b in ab.DefaultIfEmpty()
                            select new { key = a, pending = b == null ? 0 : b.pending, processed = b == null ? 0 : b.processed };

                var statusList = serviceorder.GroupBy(c => c.Status).Select(c => new
                {
                    key = c.Key,
                    count = c.Count()
                }).ToList();
                var rate = from a in category
                           join b in statusList on a.DtValue equals b.key.ToString() into ab
                           from b in ab.DefaultIfEmpty()
                           select new
                           {
                               dtValue = a.DtValue,
                               name = a.Name,
                               rate = b == null ? 0 : Math.Round(b.count / Convert.ToDecimal(serviceorder.Count()), 2)
                           };

                reportResp.Add(new { type = "ServiceWorkOrder", resultList = histogram, rateList = rate });
                _cacheContext.Set<dynamic>("HomePageChartInfo", reportResp, DateTime.Now.AddDays(1).Date);
                result.Data = reportResp;
            }
            return result;
        }

        private void CalculateCompare(ref decimal weekCompare, ref decimal dayCompare, List<DateTime?> list)
        {
            var now = DateTime.Now;
            var startdate = now.AddDays(-14).Date;
            var lastWeekDate = now.AddDays(-7).Date;
            var enddate = now.Date;
            var lastlastDate = now.AddDays(-2).Date;
            var lastDate = now.AddDays(-1).Date;

            var lastlastWeek = list.Where(c => c < lastWeekDate).Count();//上上周
            var lastWeek = list.Where(c => c >= lastWeekDate && c < enddate).Count();//上周
            var lastlastday = list.Where(c => c >= lastlastDate && c < lastDate).Count();//前天
            var lastDay = list.Where(c => c >= lastDate && c < now.Date).Count();//昨天

            weekCompare = lastWeek == 0 ? -lastlastWeek : Math.Round((lastWeek - lastlastWeek) / Convert.ToDecimal(lastWeek), 2);
            dayCompare = lastDay == 0 ? -lastlastday : Math.Round((lastDay - lastlastday) / Convert.ToDecimal(lastDay), 2);

        }

        /// <summary>
        /// 呼叫来源
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> CallSource(QueryReportReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();
            var workOrder = UnitWork.Find<ServiceWorkOrder>(null)
                                .WhereIf(!string.IsNullOrWhiteSpace(req.Year), c => c.CreateTime.Value.Year == int.Parse(req.Year))
                                .WhereIf(!string.IsNullOrWhiteSpace(req.Month), c => c.CreateTime.Value.Month == int.Parse(req.Month));
            List<string> currentUser = new List<string>();
            if (!loginContext.Roles.Any(r => r.Name.Equals("呼叫中心-查看")) && !loginContext.Roles.Any(r => r.Name.Equals("客服主管")) && !loginContext.Roles.Any(r => r.Name.Equals("总经理")) && loginContext.User.Account != Define.SYSTEM_USERNAME)
            {
                var orgRole = await UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.Key == Define.ORGROLE && c.FirstId == loginContext.User.Id).FirstOrDefaultAsync();
                if (orgRole != null)//查看本部下数据
                {
                    var orgId = orgRole.SecondId;
                    var userIds = await UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.SecondId == orgId && c.Key == Define.USERORG).Select(c => c.FirstId).ToListAsync();
                    //serviceOrder = serviceOrder.Where(r => userIds.Contains(r.CreateUserId));
                    workOrder = workOrder.Where(c => userIds.Contains(c.CurrentUserNsapId));
                    //currentUser.AddRange(userIds);
                }
                else
                {
                    //serviceOrder = serviceOrder.Where(r => r.CreateUserId.Equals(loginContext.User.Id));
                    workOrder = workOrder.Where(r => r.CurrentUserNsapId.Equals(loginContext.User.Id));
                    //currentUser.Add(loginContext.User.Id);
                }
            };
            var workOrderIds = await workOrder.Select(c => c.ServiceOrderId).ToListAsync();
            var serviceOrderObj = await UnitWork.Find<ServiceOrder>(c => workOrderIds.Contains(c.Id))
                                .WhereIf(!string.IsNullOrWhiteSpace(req.Year), c => c.CreateTime.Value.Year == int.Parse(req.Year))
                                .WhereIf(!string.IsNullOrWhiteSpace(req.Month), c => c.CreateTime.Value.Month == int.Parse(req.Month))
                                .ToListAsync();

            var groupby = serviceOrderObj.GroupBy(c => c.FromId).Select(c => new { FromId = c.Key, Count = c.Count() }).ToList();
            var totalCount = serviceOrderObj.Count();
            var tel = groupby.Where(g => g.FromId == 1).Sum(g => g.Count).ToDecimal();
            var app = groupby.Where(g => g.FromId == 6).Sum(g => g.Count).ToDecimal();
            var other = groupby.Where(g => g.FromId != 6 && g.FromId != 1).Sum(g => g.Count).ToDecimal();

            result.Data = new
            {
                Tel = tel,
                App = app,
                Other = other,
            };
            return result;
        }

        /// <summary>
        /// 服务呼叫
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> ServiceCall(QueryReportReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();
            var startTime = req.StartTime ?? DateTime.Now.Date;
            var endTime = req.EndTime ?? DateTime.Now.AddDays(1).Date;
            var serviceOrder = await UnitWork.Find<ServiceOrder>(c => c.CreateTime >= startTime && c.CreateTime < endTime && c.VestInOrg == req.VestInOrg).ToListAsync();
            var serviceOrderId = serviceOrder.Select(c => c.Id).ToList();

            var serviceWorkOrder = UnitWork.Find<ServiceWorkOrder>(c => serviceOrderId.Contains(c.ServiceOrderId));

            if (!loginContext.Roles.Any(r => r.Name.Equals("呼叫中心-查看")) && !loginContext.Roles.Any(r => r.Name.Equals("客服主管")) && !loginContext.Roles.Any(r => r.Name.Equals("总经理")) && loginContext.User.Account != Define.SYSTEM_USERNAME)
            {
                var orgRole = await UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.Key == Define.ORGROLE && c.FirstId == loginContext.User.Id).FirstOrDefaultAsync();
                if (orgRole != null)//查看本部下数据
                {
                    var orgId = orgRole.SecondId;
                    var userIds = await UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.SecondId == orgId && c.Key == Define.USERORG).Select(c => c.FirstId).ToListAsync();
                    serviceWorkOrder = serviceWorkOrder.Where(r => userIds.Contains(r.CurrentUserNsapId));
                }
                else
                {
                    serviceWorkOrder = serviceWorkOrder.Where(r => r.CurrentUserNsapId.Equals(loginContext.User.Id));
                }
            };

            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ServiceWorkOrderStatus") && u.Enable == false && u.DtValue != "").Select(u => new { u.DtValue, u.Name }).ToListAsync();
            var serviceWorkOrderObj = await serviceWorkOrder.ToListAsync();
            var nsapUserId = serviceWorkOrderObj.Select(c => c.CurrentUserNsapId).ToList();
            var userInfo = await (from a in UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.Key == Define.USERORG && nsapUserId.Contains(c.FirstId))
                                  join b in UnitWork.Find<OpenAuth.Repository.Domain.Org>(null)
                                                .WhereIf(req.OrgName != null && req.OrgName.Count > 0, c => req.OrgName.Contains(c.Name)) on a.SecondId equals b.Id //into ab
                                  //from b in ab.DefaultIfEmpty()
                                  select new { a.FirstId, b.CascadeId, b.Name }).ToListAsync();
            var userInfoOnly = userInfo.GroupBy(c => c.FirstId).Select(c => c.OrderByDescending(o => o.CascadeId).First()).ToList();

            var query = from a in userInfoOnly
                        join b in serviceWorkOrderObj on a.FirstId equals b.CurrentUserNsapId into ab
                        from b in ab.DefaultIfEmpty()
                        select new { a, b };

            var groupbyOrg = query.GroupBy(c => c.a.Name).Select(c => new
            {
                OrgName = c.Key,
                Deatil = c.GroupBy(d => d.b.Status).Select(d => new
                {
                    Status = d.Key,
                    Count = d.Count()
                }).ToList()
            }).ToList();

            List<dynamic> detail = new List<dynamic>();
            //foreach (var item in groupbyOrg)
            //{
            //    var g = from a in CategoryList
            //            join b in item.Deatil on a.DtValue equals b.Status.ToString() into ab
            //            from b in ab.DefaultIfEmpty()
            //            select new { Name = a.Name, Count = b == null ? 0 : b.Count };
            //    detail.Add(new { OrgName = item.OrgName, Detail = g });
            //}
            groupbyOrg.ForEach(c =>
            {
                var g = from a in CategoryList
                        join b in c.Deatil on a.DtValue equals b.Status.ToString() into ab
                        from b in ab.DefaultIfEmpty()
                        select new { Name = a.Name, Count = b == null ? 0 : b.Count };
                detail.Add(new { OrgName = c.OrgName, Detail = g });
            });

            result.Data = detail;

            return result;
        }
        /// <summary>
        /// 报销金额
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> Reimburseinfo(QueryReportReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();
            var reimburseInfos = UnitWork.Find<ReimburseInfo>(null).WhereIf(!string.IsNullOrWhiteSpace(req.Year), c => c.CreateTime.Year == int.Parse(req.Year));
            List<string> currentUser = new List<string>();
            if (!loginContext.Roles.Any(r => r.Name.Equals("呼叫中心-查看")) && !loginContext.Roles.Any(r => r.Name.Equals("客服主管")) && !loginContext.Roles.Any(r => r.Name.Equals("总经理")) && loginContext.User.Account != Define.SYSTEM_USERNAME)
            {
                var orgRole = await UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.Key == Define.ORGROLE && c.FirstId == loginContext.User.Id).FirstOrDefaultAsync();
                if (orgRole != null)//查看本部下数据
                {
                    var orgId = orgRole.SecondId;
                    var userIds = await UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.SecondId == orgId && c.Key == Define.USERORG).Select(c => c.FirstId).ToListAsync();
                    reimburseInfos = reimburseInfos.Where(r => userIds.Contains(r.CreateUserId));
                    currentUser.AddRange(userIds);
                }
                else
                {
                    reimburseInfos = reimburseInfos.Where(r => r.CreateUserId.Equals(loginContext.User.Id));
                    currentUser.Add(loginContext.User.Id);
                }
            };

            var reimburseInfosObj = await reimburseInfos.ToListAsync();

            var currsoids = currentUser.Count > 0 ? await UnitWork.Find<ServiceWorkOrder>(c => currentUser.Contains(c.CurrentUserNsapId)).Select(c => c.ServiceOrderId).Distinct().ToListAsync() : null;
            var serverOrderIds = reimburseInfosObj.Select(c => c.ServiceOrderId).ToList();
            var expends = await UnitWork.Find<ServiceDailyExpends>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(req.Year), c => c.CreateTime.Value.Year == int.Parse(req.Year))
                //.WhereIf(!string.IsNullOrWhiteSpace(request.CreateUserName), r => UserIds.Contains(r.CreateUserId))
                .WhereIf(currsoids != null, r => currsoids.Contains(r.ServiceOrderId))//不能查看全部的则查看自己或部门下的服务单关联的日费
                .WhereIf(currsoids == null, s => !serverOrderIds.Contains(s.ServiceOrderId))//全部日费
                .ToListAsync();
            var expendsGroupbyMonth = expends.GroupBy(c => c.CreateTime.Value.Month).Select(c => new { Month = c.Key, Moeny = c.Sum(m => m.TotalMoney) }).ToList();

            var groupbyMonth = reimburseInfosObj.GroupBy(c => c.CreateTime.Month).Select(c => new
            {
                Month = c.Key,
                Child = c.GroupBy(m => m.RemburseStatus).Select(m => new
                {
                    RemburseStatus = m.Key,
                    Moeny = m.Sum(s => s.TotalMoney)
                }).ToList()
            }).ToList();

            var query = from a in monthTemp
                        join b in groupbyMonth on a.ID equals b.Month into ab
                        from b in ab.DefaultIfEmpty()
                        select new { a.ID, Month = a.Name, Detail = b != null ? b.Child : null };
            List<dynamic> detail = new List<dynamic>();
            query.ToList().ForEach(c =>
            {
                var daliyFee = expendsGroupbyMonth.Where(e => e.Month == c.ID).Select(e => e.Moeny).Sum();
                var approval = c.Detail == null ? 0 : c.Detail?.Where(d => d.RemburseStatus != 8 && d.RemburseStatus != 9).Sum(d => d.Moeny);
                detail.Add(new
                {
                    Month = c.Month,
                    Paid = c.Detail == null ? 0 : c.Detail.Where(d => d.RemburseStatus == 9).Sum(d => d.Moeny),
                    Paying = c.Detail == null ? 0 : c.Detail?.Where(d => d.RemburseStatus == 8).Sum(d => d.Moeny),
                    Approval = approval + daliyFee,
                });
            });

            result.Data = new
            {
                TotalMoney = reimburseInfosObj.Sum(c => c.TotalMoney) + expendsGroupbyMonth.Sum(c => c.Moeny),
                Detail = detail
            };
            return result;
        }

        /// <summary>
        /// 销售金额
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> SaleAmount(QueryReportReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();
            var quotation = UnitWork.Find<Quotation>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(req.Year), c => c.CreateTime.Year == int.Parse(req.Year));

            if (!loginContext.Roles.Any(r => r.Name.Equals("呼叫中心-查看")) && !loginContext.Roles.Any(r => r.Name.Equals("客服主管")) && !loginContext.Roles.Any(r => r.Name.Equals("总经理")) && loginContext.User.Account != Define.SYSTEM_USERNAME)
            {
                var orgRole = await UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.Key == Define.ORGROLE && c.FirstId == loginContext.User.Id).FirstOrDefaultAsync();
                if (orgRole != null)//查看本部下数据
                {
                    var orgId = orgRole.SecondId;
                    var userIds = await UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.SecondId == orgId && c.Key == Define.USERORG).Select(c => c.FirstId).ToListAsync();
                    quotation = quotation.Where(r => userIds.Contains(r.CreateUserId));
                }
                else
                {
                    quotation = quotation.Where(r => r.CreateUserId.Equals(loginContext.User.Id));
                }
            };

            var quotationObj = await quotation.ToListAsync();

            var groupby = quotationObj.GroupBy(c => c.CreateTime.Month).Select(c => new { Month = c.Key, Detail = c }).ToList();

            var query = from a in monthTemp
                        join b in groupby on a.ID equals b.Month into ab
                        from b in ab.DefaultIfEmpty()
                        select new { Month = a.Name, Detail = b != null ? b.Detail : null };

            List<dynamic> detail = new List<dynamic>();
            query.ToList().ForEach(c =>
            {
                detail.Add(new
                {
                    Month = c.Month,
                    NoFinlish = c.Detail == null ? 0 : c.Detail.Where(d => d.DeliveryMethod != "1").Sum(d => d.TotalMoney),
                    Finlish = c.Detail == null ? 0 : c.Detail?.Where(d => d.DeliveryMethod == "1" && d.Status == 2).Sum(d => d.TotalMoney),//款到发货并为出库单
                });
            });

            result.Data = new
            {
                TotalMoney = quotation.Sum(c => c.TotalMoney),
                Detail = detail
            };
            return result;
        }

        /// <summary>
        /// 个代金额
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> SettlementAmount(QueryReportReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();
            var outsourc = UnitWork.Find<Outsourc>(null).WhereIf(!string.IsNullOrWhiteSpace(req.Year), c => c.CreateTime.Value.Year == int.Parse(req.Year));

            if (!loginContext.Roles.Any(r => r.Name.Equals("呼叫中心-查看")) && !loginContext.Roles.Any(r => r.Name.Equals("客服主管")) && !loginContext.Roles.Any(r => r.Name.Equals("总经理")) && loginContext.User.Account != Define.SYSTEM_USERNAME)
            {
                var orgRole = await UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.Key == Define.ORGROLE && c.FirstId == loginContext.User.Id).FirstOrDefaultAsync();
                if (orgRole != null)//查看本部下数据
                {
                    var orgId = orgRole.SecondId;
                    var userIds = await UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.SecondId == orgId && c.Key == Define.USERORG).Select(c => c.FirstId).ToListAsync();
                    outsourc = outsourc.Where(r => userIds.Contains(r.CreateUserId));
                }
                else
                {
                    outsourc = outsourc.Where(r => r.CreateUserId.Equals(loginContext.User.Id));
                }
            };

            var outsourcObj = await outsourc.ToListAsync();
            var flowInstaceId = outsourcObj.Select(c => c.FlowInstanceId).ToList();
            var flowInstace = await UnitWork.Find<FlowInstance>(c => flowInstaceId.Contains(c.Id)).ToListAsync();
            var queryMerge = from a in outsourcObj
                             join b in flowInstace on a.FlowInstanceId equals b.Id into ab
                             from b in ab.DefaultIfEmpty()
                             select new { a, ActivityName = b == null ? null : b.ActivityName };
            var groupbyMonth = queryMerge.GroupBy(c => c.a.CreateTime.Value.Month).Select(c => new
            {
                Month = c.Key,
                Child = c.GroupBy(g => g.ActivityName).Select(g => new
                {
                    ActivityName = g.Key,
                    Money = g.Sum(g => g.a.TotalMoney)
                }).ToList()
            }).ToList();

            var query = from a in monthTemp
                        join b in groupbyMonth on a.ID equals b.Month into ab
                        from b in ab.DefaultIfEmpty()
                        select new { Month = a.Name, Detail = b == null ? null : b.Child };

            List<dynamic> detail = new List<dynamic>();
            query.ToList().ForEach(c =>
            {
                detail.Add(new
                {
                    Month = c.Month,
                    Paid = c.Detail == null ? 0 : c.Detail.Where(d => d.ActivityName == "结束").Sum(d => d.Money),
                    Paying = c.Detail == null ? 0 : c.Detail?.Where(d => d.ActivityName == "财务支付").Sum(d => d.Money),
                    Approval = c.Detail == null ? 0 : c.Detail?.Where(d => d.ActivityName != "财务支付" && d.ActivityName != "结束").Sum(d => d.Money)
                });
            });

            result.Data = new
            {
                TotalMoney = outsourcObj.Sum(c => c.TotalMoney),
                Detail = detail
            };
            return result;
        }

        /// <summary>
        /// 获取客诉服务
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetServiceOrderInfo(QueryReportReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();
            var workOrder = UnitWork.Find<ServiceWorkOrder>(null)
                                .WhereIf(!string.IsNullOrWhiteSpace(req.Year), c => c.CreateTime.Value.Year == int.Parse(req.Year))
                                .WhereIf(!string.IsNullOrWhiteSpace(req.Month), c => c.CreateTime.Value.Month == int.Parse(req.Month));
            if (!loginContext.Roles.Any(r => r.Name.Equals("呼叫中心-查看")) && !loginContext.Roles.Any(r => r.Name.Equals("客服主管")) && !loginContext.Roles.Any(r => r.Name.Equals("总经理")) && loginContext.User.Account != Define.SYSTEM_USERNAME)
            {
                var orgRole = await UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.Key == Define.ORGROLE && c.FirstId == loginContext.User.Id).FirstOrDefaultAsync();
                if (orgRole != null)//查看本部下数据
                {
                    var orgId = orgRole.SecondId;
                    var userIds = await UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.SecondId == orgId && c.Key == Define.USERORG).Select(c => c.FirstId).ToListAsync();
                    workOrder = workOrder.Where(c => userIds.Contains(c.CurrentUserNsapId));
                }
                else
                {
                    workOrder = workOrder.Where(r => r.CurrentUserNsapId.Equals(loginContext.User.Id));
                }
            };
            var workOrderIds = await workOrder.Select(c => c.ServiceOrderId).ToListAsync();
            var serviceOrder = await UnitWork.Find<ServiceOrder>(c => c.VestInOrg == 1 && c.CreateTime != null && workOrderIds.Contains(c.Id))
                .WhereIf(!string.IsNullOrWhiteSpace(req.Year), c => c.CreateTime.Value.Year == int.Parse(req.Year)).Select(c => new { c.CreateTime }).ToListAsync();
            var groupby = serviceOrder.GroupBy(c => c.CreateTime.Value.Month).Select(c => new { Month = c.Key, Count = c.Count() }).ToList();
            var query = from a in monthTemp
                        join b in groupby on a.ID equals b.Month into ab
                        from b in ab.DefaultIfEmpty()
                        select new { Month = a.Name, Count = b != null ? b.Count : 0 };
            result.Data = new
            {
                TotalCount = serviceOrder.Count(),
                Detail = query.ToList()
            };
            return result;
        }
        #endregion

        #region 服务呼叫报表
        /// <summary>
        /// 服务呼叫分布
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> ServiceOrderDistribution(QueryReportReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();

            var serviceOrder = await UnitWork.Find<ServiceOrder>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(req.Year), c => c.CreateTime.Value.Year == int.Parse(req.Year))
                .WhereIf(!string.IsNullOrWhiteSpace(req.Month), c => c.CreateTime.Value.Month == int.Parse(req.Month))
                .Select(c=>c.VestInOrg)
                .ToListAsync();
            var groupbyValue = serviceOrder.GroupBy(c => c).Select(c => new { c.Key, Count = c.Count() }).ToList();
            var listTemp = new List<dynamic> { new { ID = 1, Name = "客诉单" }, new { ID = 2, Name = "维修单" }, new { ID = 3, Name = "行政单" } };
            var query = from a in listTemp
                        join b in groupbyValue on a.ID equals b.Key into ab
                        from b in ab.DefaultIfEmpty()
                        select new { a.Name, Count = b == null ? 0 : b.Count };
            result.Data = query;
            return result;
        }

        /// <summary>
        /// 服务呼叫来源
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> ServiceOrderSource(QueryReportReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();
            var CategoryList = await UnitWork.Find<Category>(c => c.TypeId == "SYS_CallTheSource").Select(c => new { c.DtValue, c.Name }).ToListAsync();

            var serviceOrder = await UnitWork.Find<ServiceOrder>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(req.Year), c => c.CreateTime.Value.Year == int.Parse(req.Year))
                .WhereIf(!string.IsNullOrWhiteSpace(req.Month), c => c.CreateTime.Value.Month == int.Parse(req.Month))
                .Select(c => c.FromId)
                .ToListAsync();
            var groupbyValue = serviceOrder.GroupBy(c => c).Select(c => new { c.Key, Count = c.Count() }).ToList();
            var query = from a in CategoryList
                        join b in groupbyValue on a.DtValue equals b.Key.ToString() into ab
                        from b in ab.DefaultIfEmpty()
                        select new { a.Name, Count = b == null ? 0 : b.Count };
            result.Data = query;
            return result;
        }

        /// <summary>
        /// 服务呼叫状态和问题类型分析
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> ServiceOrderStatusAndProblemType(QueryReportReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //TableData result = new TableData();

            var serviceOrder = await UnitWork.Find<ServiceOrder>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(req.Year), c => c.CreateTime.Value.Year == int.Parse(req.Year))
                .WhereIf(!string.IsNullOrWhiteSpace(req.Month), c => c.CreateTime.Value.Month == int.Parse(req.Month))
                .Select(c => c.Id)
                .ToListAsync();
            var serviceWorkOrder = await UnitWork.Find<ServiceWorkOrder>(c => serviceOrder.Contains(c.ServiceOrderId))
                .Include(c => c.ProblemType)
                .Select(c => new { c.CurrentUserNsapId, c.CurrentUser, c.Status, c.ProblemType })
                .ToListAsync();
            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ServiceWorkOrderStatus") && u.Enable == false && u.DtValue != "").Select(u => new { u.DtValue, u.Name }).ToListAsync();
            List<ProblemType> problemTypes = new List<ProblemType>();
            List<dynamic> detail = new List<dynamic>();
            if (req.Range == "user")
            {
                if (req.Name!=null && req.Name.Count>0)
                    serviceWorkOrder = serviceWorkOrder.Where(c => req.Name.Contains(c.CurrentUser)).ToList();

                var groupbyUser = serviceWorkOrder.GroupBy(c => c.CurrentUser).Select(c => new
                {
                    UserName = c.Key,
                    Detail = c.GroupBy(d => d.Status).Select(d => new
                    {
                        Status = d.Key,
                        Count = d.Count()
                    }).ToList()
                }).ToList();

                groupbyUser.ForEach(c =>
                {
                    var g = from a in CategoryList
                            join b in c.Detail on a.DtValue equals b.Status.ToString() into ab
                            from b in ab.DefaultIfEmpty()
                            select new { Name = a.Name, Count = b == null ? 0 : b.Count };
                    detail.Add(new { Name = c.UserName, Detail = g });
                });

                //问题类型
                serviceWorkOrder.ForEach(c =>
                {
                    if (c.ProblemType != null)
                    {
                        problemTypes.Add(c.ProblemType);
                    }
                });
            }
            else if (req.Range == "org")
            {
                var nsapUserId = serviceWorkOrder.Select(c => c.CurrentUserNsapId).ToList();
                var userInfo = await (from a in UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.Key == Define.USERORG && nsapUserId.Contains(c.FirstId))
                                      join b in UnitWork.Find<OpenAuth.Repository.Domain.Org>(null) on a.SecondId equals b.Id //into ab
                                      //from b in ab.DefaultIfEmpty()
                                      select new { a.FirstId, b.CascadeId, b.Name }).ToListAsync();
                var userInfoOnly = userInfo.GroupBy(c => c.FirstId).Select(c => c.OrderByDescending(o => o.CascadeId).First()).ToList();

                var query = from a in userInfoOnly
                            join b in serviceWorkOrder on a.FirstId equals b.CurrentUserNsapId into ab
                            from b in ab.DefaultIfEmpty()
                            select new { a, b };
                //部门筛选
                if (req.Name != null && req.Name.Count > 0)
                    query = query.Where(c => req.Name.Contains(c.a.Name)).ToList();

                var groupbyOrg = query.GroupBy(c => c.a.Name).Select(c => new
                {
                    OrgName = c.Key,
                    Deatil = c.GroupBy(d => d.b.Status).Select(d => new
                    {
                        Status = d.Key,
                        Count = d.Count()
                    }).ToList()
                }).ToList();

                groupbyOrg.ForEach(c =>
                {
                    var g = from a in CategoryList
                            join b in c.Deatil on a.DtValue equals b.Status.ToString() into ab
                            from b in ab.DefaultIfEmpty()
                            select new { Name = a.Name, Count = b == null ? 0 : b.Count };
                    detail.Add(new { Name = c.OrgName, Detail = g });
                });

                query.ToList().ForEach(c =>
                {
                    if (c.b.ProblemType!=null)
                    {
                        problemTypes.Add(c.b.ProblemType);
                    }
                });
            }
            var problemTypesData = problemTypes.GroupBy(c => c.Name).Select(c => new { Name = c.Key, Count = c.Count() }).ToList();
            return new TableData
            {
                Data = new
                {
                    Status = detail,
                    ProblemTypes = problemTypesData
                }
            };
        }

        /// <summary>
        /// 催办次数
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> UrgingTimes(QueryReportReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();
            var serviceMessage = await UnitWork.Find<ServiceOrderMessage>(c => c.Content.Contains("催办"))
                .WhereIf(!string.IsNullOrWhiteSpace(req.Year), c => c.CreateTime.Value.Year == int.Parse(req.Year))
                .WhereIf(!string.IsNullOrWhiteSpace(req.Month), c => c.CreateTime.Value.Month == int.Parse(req.Month))
                .ToListAsync();
            if (req.Range == "user")
            {
                if (req.Name != null && req.Name.Count > 0)
                    serviceMessage = serviceMessage.Where(c => req.Name.Contains(c.Replier)).ToList();
                result.Data = serviceMessage.GroupBy(c => c.Replier).Select(c => new { Name = c.Key, Count = c.Count() }).ToList();
            }
            else
            {
                var nsapUserId = serviceMessage.Select(c => c.ReplierId).ToList();
                var userInfo = await (from a in UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.Key == Define.USERORG && nsapUserId.Contains(c.FirstId))
                                      join b in UnitWork.Find<OpenAuth.Repository.Domain.Org>(null) on a.SecondId equals b.Id //into ab
                                      //from b in ab.DefaultIfEmpty()
                                      select new { a.FirstId, b.CascadeId, b.Name }).ToListAsync();
                var userInfoOnly = userInfo.GroupBy(c => c.FirstId).Select(c => c.OrderByDescending(o => o.CascadeId).First()).ToList();

                if (req.Name != null && req.Name.Count > 0)
                    userInfoOnly = userInfoOnly.Where(c => req.Name.Contains(c.Name)).ToList();

                var query = from a in userInfoOnly
                            join b in serviceMessage on a.FirstId equals b.ReplierId into ab
                            from b in ab.DefaultIfEmpty()
                            select new { a, b };
                result.Data = query.GroupBy(c => c.a.Name).Select(c => new { Name = c.Key, Count = c.Count() }).ToList();
            }
            return result;
        }

        /// <summary>
        /// 呼叫主题进度分析
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> ServiceThemeProgress(QueryReportReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var startTime = req.StartTime ?? DateTime.Now.Date;
            var endTime = req.EndTime ?? DateTime.Now.AddDays(1).Date;
            var serviceOrder = await UnitWork.Find<ServiceOrder>(c=>c.CreateTime>= startTime && c.CreateTime< endTime)
                .Select(c => c.Id)
                .ToListAsync();
            var serviceWorkOrder = await UnitWork.Find<ServiceWorkOrder>(c => serviceOrder.Contains(c.ServiceOrderId))
                .Select(c => new { c.FromTheme, c.Status })
                .ToListAsync();

            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ServiceWorkOrderStatus") && u.Enable == false && u.DtValue != "").Select(u => new { u.DtValue, u.Name }).ToListAsync();

            List<ServiceWorkOrderFromTheme> fromThemes = new List<ServiceWorkOrderFromTheme>();
            serviceWorkOrder.ForEach(c =>
            {
                var theme = GetServiceTroubleAndSolution(c.FromTheme);
                theme.ForEach(t =>
                {
                    fromThemes.Add(new ServiceWorkOrderFromTheme { Description = t.Description, Status = c.Status });
                });
            });
            if (req.Name != null && req.Name.Count > 0)
            {
                fromThemes = fromThemes.Where(c => req.Name.Contains(c.Description)).ToList();
            }

            var groupbyTheme = fromThemes.GroupBy(c => c.Description).Select(c => new
            {
                Name = c.Key,
                Detail = c.GroupBy(d => d.Status).Select(d => new
                {
                    Status = d.Key,
                    Count = d.Count()
                }).ToList()
            }).ToList();

            List<dynamic> detail = new List<dynamic>();
            groupbyTheme.ForEach(c =>
            {
                var g = from a in CategoryList
                        join b in c.Detail on a.DtValue equals b.Status.ToString() into ab
                        from b in ab.DefaultIfEmpty()
                        select new { Name = a.Name, Count = b == null ? 0 : b.Count };
                detail.Add(new { Name = c.Name, Detail = g });
            });

            return new TableData
            {
                Data = detail
            };
        }

        /// <summary>
        /// 根据生产部门获取服务呼叫客诉单的信息,按服务方式分类(电话,上门)
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetServiceCallInfo(QueryReportReq req)
        {
            var result = new TableData();

            var startTime = req.StartTime ?? DateTime.Now.Date;
            var endTime = req.EndTime ?? startTime.AddDays(1).Date;

            //因为服务单信息跟部门信息不在一个服务器上,所以处理思路是:服务单->序列号(这个在erp4.0),序列号->部门(这个在erp3.0)

            //根据票据类型和时间,筛选出序列号
            var severData = await (from s in UnitWork.Find<ServiceOrder>(x => x.VestInOrg == req.VestInOrg && x.CreateTime >= startTime && x.CreateTime < endTime)
                                   join sw in UnitWork.Find<ServiceWorkOrder>(x => x.Status >= 2)
                                   on s.Id equals sw.ServiceOrderId
                                   select new { s.Id, sw.ServiceMode, sw.FromTheme, sw.ManufacturerSerialNumber }).ToListAsync();

            //序列号去重
            var serialData = severData.GroupBy(x => x.ManufacturerSerialNumber).Select(x => x.Key);

            //根据序列号找出对应的部门(这个查找方式是从erp3.0找过来的,还有别的方式?)
            var deptsInfo = await (from a in UnitWork.Find<store_oitl>(null)
                                   join b in UnitWork.Find<store_itl1>(null) on new { a.sbo_id, a.ItemCode, a.LogEntry } equals new { b.sbo_id, b.ItemCode, b.LogEntry } into ab
                                   from bDefault in ab.DefaultIfEmpty()
                                   join c in UnitWork.Find<store_osrn>(null) on new { bDefault.sbo_id, bDefault.ItemCode, bDefault.SysNumber } equals new { c.sbo_id, c.ItemCode, c.SysNumber } into bc
                                   from cDefault in bc.DefaultIfEmpty()
                                   join d in UnitWork.Find<product_owor>(null) on new { sbo_id = a.sbo_id.Value, DocEntry = a.BaseEntry.Value } equals new { d.sbo_id, d.DocEntry }
                                   where new int[] { 15, 59 }.Contains(a.DocType.Value) && a.BaseType.Value == 202 && serialData.Contains(cDefault.MnfSerial)
                                   select new { cDefault.MnfSerial, d.U_WO_LTDW }).ToListAsync();

            //处理数据(例如:将P6,P6-李xx,p6处理为一个部门)
            var depts = deptsInfo.Select(x => new
            {
                dept = (x.U_WO_LTDW.Contains("-") ? x.U_WO_LTDW.Split('-')[0] : x.U_WO_LTDW).ToUpper(),
                serial = x.MnfSerial
            });

            //根据传入部门过滤数据
            if (req.OrgName?.Count() > 0)
            {
                depts = depts.Where(x => req.OrgName.Contains(x.dept));
            }

            //去重
            var data = (from a in severData
                        join b in depts on a.ManufacturerSerialNumber equals b.serial
                        select new { a.Id, a.ServiceMode, b.dept }).Distinct();

            //这个我没在数据库中找到维护的表,所以固定写在代码里
            var serviceTypes = new[]
            {
                new { name = "上门服务",value = 1},
                new { name = "电话服务",value = 2},
            };

            var testData = data.Where(x => x.ServiceMode != null).GroupBy(x => x.dept).Select(x => new
            {
                OrgName = x.Key,
                Deatil = x.GroupBy(d => d.ServiceMode).Select(d => new
                {
                    Status = d.Key,
                    Count = d.Count()
                })
            });

            //分类没有则为0
            List<dynamic> detail = new List<dynamic>();
            foreach(var item in testData)
            {
                var g = from a in serviceTypes
                        join b in item.Deatil on a.value equals b.Status into temp
                        from t in temp.DefaultIfEmpty()
                        select new { Name = a.name, Count = t == null ? 0 : t.Count };

                detail.Add(new { OrgName = item.OrgName, Detail = g });
            }

            result.Data = detail;
            result.Count = detail.Count();

            return result;
        }

        /// <summary>
        /// 统计生产部门的呼叫主题
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetServiceThemeInfo(QueryReportReq req)
        {
            var result = new TableData();

            var startTime = req.StartTime ?? DateTime.Now.Date;
            var endTime = req.EndTime ?? startTime.AddDays(1).Date;

            //因为服务单信息跟部门信息不在一个服务器上,所以处理思路是:服务单->序列号(这个在erp4.0),序列号->部门(这个在erp3.0)

            //根据票据类型和时间,筛选出序列号
            var severData = await (from s in UnitWork.Find<ServiceOrder>(x => x.VestInOrg == req.VestInOrg && x.CreateTime >= startTime && x.CreateTime < endTime)
                                   join sw in UnitWork.Find<ServiceWorkOrder>(x => x.Status >= 2)
                                   on s.Id equals sw.ServiceOrderId
                                   select new { s.Id, sw.ServiceMode, sw.FromTheme, sw.ManufacturerSerialNumber }).ToListAsync();

            //序列号去重
            var serialData = severData.GroupBy(x => x.ManufacturerSerialNumber).Select(x => x.Key);

            //根据序列号找出对应的部门(这个查找方式是从erp3.0找过来的,还有别的方式?)
            var deptsInfo = await (from a in UnitWork.Find<store_oitl>(null)
                                   join b in UnitWork.Find<store_itl1>(null) on new { a.sbo_id, a.ItemCode, a.LogEntry } equals new { b.sbo_id, b.ItemCode, b.LogEntry } into ab
                                   from bDefault in ab.DefaultIfEmpty()
                                   join c in UnitWork.Find<store_osrn>(null) on new { bDefault.sbo_id, bDefault.ItemCode, bDefault.SysNumber } equals new { c.sbo_id, c.ItemCode, c.SysNumber } into bc
                                   from cDefault in bc.DefaultIfEmpty()
                                   join d in UnitWork.Find<product_owor>(null) on new { sbo_id = a.sbo_id.Value, DocEntry = a.BaseEntry.Value } equals new { d.sbo_id, d.DocEntry }
                                   where new int[] { 15, 59 }.Contains(a.DocType.Value) && a.BaseType.Value == 202 && serialData.Contains(cDefault.MnfSerial)
                                   select new { cDefault.MnfSerial, d.U_WO_LTDW }).ToListAsync();

            //处理数据(例如:将P6,P6-李xx,p6处理为一个部门)
            var depts = deptsInfo.Select(x => new
            {
                dept = (x.U_WO_LTDW.Contains("-") ? x.U_WO_LTDW.Split('-')[0] : x.U_WO_LTDW).ToUpper(),
                serial = x.MnfSerial
            });

            //根据传入部门过滤数据
            if (req.OrgName?.Count() > 0)
            {
                depts = depts.Where(x => req.OrgName.Contains(x.dept));
            }

            //查询主题(只查询服务方式不为空的数据)
            var data = from a in severData.Where(x => x.ServiceMode != null)
                       join b in depts on a.ManufacturerSerialNumber equals b.serial
                       select new
                       {
                           a.Id,
                           a.ServiceMode,
                           Theme = GetServiceTroubleAndSolution(a.FromTheme).Select(x => new { x.ThemeId, x.Description, x.Code }),
                           b.dept
                       };

            //将一对多拆分成一对一
            List<DeptServiceTheme> resultData = new List<DeptServiceTheme>();
            foreach (var idItem in data.Where(x => x.ServiceMode != null))
            {
                foreach (var theme in idItem.Theme)
                {
                    resultData.Add(new DeptServiceTheme
                    {
                        ServiceId = idItem.Id,
                        ServiceMode = idItem.ServiceMode.Value,
                        ServiceDept = idItem.dept,
                        ServiceThemeId = theme.ThemeId,
                        ServiceThemeCode = theme.Code,
                        ServiceThemeDesc = theme.Description,
                    });
                }
            }

            //这个我没在数据库中找到维护的表,所以固定写在代码里
            var serviceTypes = new[]
            {
                new { name = "上门服务",value = 1},
                new { name = "电话服务",value = 2},
            };

            //统计
            var testData = from d in resultData.Select(x => new { x.ServiceId, x.ServiceMode, x.ServiceThemeId, x.ServiceThemeCode, x.ServiceThemeDesc, x.ServiceDept }).Distinct()
                           group d by new { d.ServiceDept, d.ServiceThemeId }
                           into g
                           select new
                           {
                               g.Key.ServiceDept,
                               //ThemeId = GetTreeCode(g.Key.ServiceThemeId),
                               ThemeCode = g.Min(x => x.ServiceThemeCode),
                               ThemeDesc = g.Min(x => x.ServiceThemeDesc),
                               count = g.Count(x => new int[] { 1, 2 }.Contains(x.ServiceMode)),
                               detail = g.GroupBy(x => x.ServiceMode).Select(x => new
                               {
                                   serviceType = x.Key,
                                   count = x.Count()
                               })
                           };

            //分类没有则为0
            List<dynamic> detail = new List<dynamic>();
            foreach (var item in testData.OrderByDescending(x => x.count))
            {
                var g = from a in serviceTypes
                        join b in item.detail on a.value equals b.serviceType into temp
                        from t in temp.DefaultIfEmpty()
                        select new { Name = a.name, Count = t == null ? 0 : t.count };

                detail.Add(new
                {
                    ServiceDept = item.ServiceDept,
                    ThemeCode = item.ThemeCode,
                    ThemeDesc = item.ThemeDesc,
                    count = item.count,
                    Detail = g
                });
            }

            result.Data = detail;
            result.Count = detail.Count();
            
            return result;
        }
        #endregion

        private List<ServiceWorkOrderFromTheme> GetServiceTroubleAndSolution(string data)
        {
            List<ServiceWorkOrderFromTheme> result = new List<ServiceWorkOrderFromTheme>();
            if (!string.IsNullOrEmpty(data))
            {
                JArray jArray = (JArray)JsonConvert.DeserializeObject(data);
                foreach (var item in jArray)
                {
                    result.Add(new ServiceWorkOrderFromTheme
                    {
                        Description = item["description"].ToString(),
                        ThemeId = item["id"].ToString(),
                        Code = item["code"] == null ? "" : item["code"].ToString(),
                    });
                }
            }
            return result;
        }

        private string GetTreeCode(string id)
        {
            var parent = UnitWork.Find<KnowledgeBase>(x => x.Id == id).FirstOrDefault();
            if (parent == null || parent?.ParentId == "")
            {
                return parent?.Code;
            }
            else
            {

                return GetTreeCode(parent.ParentId) + "-" + parent.Code;
            }
        }
    }

    public class HomePageCardReportResp
    {
        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// 周同比
        /// </summary>
        public decimal WeekCompare { get; set; }
        /// <summary>
        /// 日同比
        /// </summary>
        public decimal DayCompare { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public List<StatusList> StatusList { get; set; }
    }

    public class StatusList
    {
        public int SId { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
    }

    public class ServiceWorkOrderFromTheme
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public int? Status { get; set; }

        public string? ThemeId { get; set; }
    }

    public class DeptServiceTheme
    {
        /// <summary>
        /// 服务id
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// 服务方式
        /// </summary>
        public int ServiceMode { get; set; }

        /// <summary>
        /// 服务主题Id
        /// </summary>
        public string ServiceThemeId { get; set; }

        /// <summary>
        /// 服务主题Code
        /// </summary>
        public string ServiceThemeCode { get; set; }

        /// <summary>
        /// 服务主题描述
        /// </summary>
        public string ServiceThemeDesc { get; set; }

        /// <summary>
        /// 服务部门
        /// </summary>
        public string ServiceDept { get; set; }
    }
}
