using Infrastructure;
using Infrastructure.Cache;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
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
        public CommonApp(IUnitWork unitWork, IAuth auth, ICacheContext cacheContext) : base(unitWork, auth)
        {
            _cacheContext = cacheContext;
        }

        /// <summary>
        /// 获取首页四个数量汇总
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetTopNumCard()
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext==null)
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
            var serviceOrder = await UnitWork.Find<ServiceOrder>(c => c.CreateTime >= startdate && c.CreateTime <= DateTime.Now).Select(c=>new {c.Id,c.CreateTime }).ToListAsync();
            CalculateCompare(ref weekCompare, ref dayCompare, serviceOrder.Select(c => c.CreateTime).ToList());//计算同比

            var serviceOrderIds = serviceOrder.Where(c => c.CreateTime >= now.Date && c.CreateTime <= now).Select(c => c.Id).ToList();
            var sow = await UnitWork.Find<ServiceWorkOrder>(c => serviceOrderIds.Contains(c.ServiceOrderId)).ToListAsync();

            var currentSowStatus = sow.GroupBy(c => c.Status).Select(c => new { c.Key, Count = c.Count() }).ToList();
            var sowStatusTemp = await UnitWork.Find<Category>(c => c.TypeId == "SYS_ServiceWorkOrderStatus" && c.SortNo > 0).Select(c => new { Id = int.Parse(c.DtValue) , Name = c.Name }).ToListAsync();

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
                          select new { Name=a.Name, Count = b == null ? 0 : b.Count };
            reportResp.Add(new { Quantity = currentRN.Count, Type = "ReturnNote", WeekCompare = weekCompare, DayCompare = dayCompare, StatusList = rnStaus });
            #endregion

            #region 报销
            var reimburseinfo=await UnitWork.Find<ReimburseInfo>(c => c.CreateTime >= startdate && c.CreateTime <= DateTime.Now).Select(c=>new {c.CreateTime,c.FlowInstanceId }).ToListAsync();
            CalculateCompare(ref weekCompare, ref dayCompare, reimburseinfo.Select(c => c?.CreateTime).ToList());//计算同比
            var currentRE= reimburseinfo.Where(c => c.CreateTime >= now.Date && c.CreateTime <= now).Select(c => c.FlowInstanceId).ToList();
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
                          select new { Name=a.Name, Count = b == null ? 0 : b.Count };
            reportResp.Add(new { Quantity = currentRE.Count, Type = "ReimburseInfo", WeekCompare = weekCompare, DayCompare = dayCompare, StatusList = reStaus });
            #endregion

            #region 销售订单
            var quotationoperationhistory=await UnitWork.Find<QuotationOperationHistory>(c => c.CreateTime >= startdate && c.CreateTime <= DateTime.Now && c.Action== "客户确认报价单").ToListAsync();
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
            } ;

            var qtStaus = from a in quotationStatusTemp
                          join b in currentQTStatus on a.ID equals b.Key into ab
                          from b in ab.DefaultIfEmpty()
                          select new { Name=a.Name, Count = b == null ? 0 : b.Count };
            reportResp.Add(new { Quantity = currentQT.Count, Type = "Quotation", WeekCompare = weekCompare, DayCompare = dayCompare, StatusList = qtStaus.GroupBy(c => c.Name).Select(c => new { Name=c.Key, Count = c.Sum(o => o.Count) }) });

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
            if (data!=null)
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
                    key=c.Key,
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
                    key=c.Key, 
                    count = c.Count() 
                }).ToList();
                var rate = from a in category
                           join b in statusList on a.DtValue equals b.key.ToString() into ab
                           from b in ab.DefaultIfEmpty()
                           select new
                           {
                               dtValue=a.DtValue,
                               name=a.Name,
                               rate = b == null ? 0 : Math.Round(b.count / Convert.ToDecimal(serviceorder.Count()), 2)
                           };

                reportResp.Add(new { type = "ServiceWorkOrder", resultList = histogram, rateList = rate });
                _cacheContext.Set<dynamic>("HomePageChartInfo", reportResp, DateTime.Now.AddDays(1).Date);
                result.Data = reportResp;
            }
            return result;
        }

        private void CalculateCompare(ref decimal weekCompare,ref decimal dayCompare,List<DateTime?> list)
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
        public decimal ID { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
    }
}
