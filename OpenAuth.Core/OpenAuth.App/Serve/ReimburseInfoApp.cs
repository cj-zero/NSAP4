using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Export;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using NStandard;
using OpenAuth.App.Interface;
using OpenAuth.App.Material;
using OpenAuth.App.Material.Request;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Serve.Request;
using OpenAuth.App.Serve.Response;
using OpenAuth.App.Workbench;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.Repository.Domain.Settlement;
using OpenAuth.Repository.Domain.Workbench;
using OpenAuth.Repository.Interface;

namespace OpenAuth.App
{
    public class ReimburseInfoApp : OnlyUnitWorkBaeApp
    {
        private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;
        private readonly FlowInstanceApp _flowInstanceApp;
        private readonly QuotationApp _quotation;
        private readonly WorkbenchApp _workbenchApp;

        //报销单据类型(0 报销单，1 出差补贴， 2 交通费用， 3 住宿补贴， 4 其他费用, 5 我的费用)
        /// <summary>
        /// 加载列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> Load(QueryReimburseInfoListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            #region 查询条件
            List<string> UserIds = new List<string>();
            List<int> ServiceOrderIds = new List<int>();
            List<string> OrgUserIds = new List<string>();
            if (!string.IsNullOrWhiteSpace(request.CreateUserName))
            {
                UserIds.AddRange(await UnitWork.Find<User>(u => u.Name.Contains(request.CreateUserName)).Select(u => u.Id).ToListAsync());
            }

            if (!string.IsNullOrWhiteSpace(request.TerminalCustomer))
            {
                ServiceOrderIds.AddRange(await UnitWork.Find<ServiceOrder>(s => s.TerminalCustomer.Contains(request.TerminalCustomer) || s.TerminalCustomerId.Contains(request.TerminalCustomer)).Select(s => s.Id).ToListAsync());
            }

            if (!string.IsNullOrWhiteSpace(request.OrgName))
            {
                var orgids = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(o => o.Name.Contains(request.OrgName)).Select(o => o.Id).ToListAsync();
                OrgUserIds.AddRange(await UnitWork.Find<Relevance>(r => orgids.Contains(r.SecondId) && r.Key == Define.USERORG).Select(r => r.FirstId).ToListAsync());
            }
            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ServiceRelations") && u.Enable == false).Select(u => u.Name).ToListAsync();

            var result = new TableData();
            var objs = UnitWork.Find<ReimburseInfo>(null).Include(r => r.ReimburseTravellingAllowances).Include(r => r.ReimurseOperationHistories);
            var ReimburseInfos = objs.WhereIf(!string.IsNullOrWhiteSpace(request.MainId), r => r.MainId.ToString().Contains(request.MainId))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.ServiceOrderId), r => r.ServiceOrderSapId.ToString().Contains(request.ServiceOrderId))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.BearToPay), r => r.BearToPay.Contains(request.BearToPay))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.Responsibility), r => r.Responsibility.Contains(request.Responsibility))
                      .WhereIf(request.StartDate != null, r => r.CreateTime > request.StartDate)
                      .WhereIf(request.EndDate != null, r => r.CreateTime < Convert.ToDateTime(request.EndDate).AddMinutes(1440))
                      //.WhereIf(!string.IsNullOrWhiteSpace(request.IsDraft.ToString()), r => r.IsDraft == request.IsDraft)
                      .WhereIf(!string.IsNullOrWhiteSpace(request.ReimburseType), r => r.ReimburseType.Equals(request.ReimburseType))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.CreateUserName), r => UserIds.Contains(r.CreateUserId))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.OrgName), r => OrgUserIds.Contains(r.CreateUserId))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.TerminalCustomer), r => ServiceOrderIds.Contains(r.ServiceOrderId))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.ServiceRelations), r => r.ServiceRelations.Contains(request.ServiceRelations))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.Status), r => r.RemburseStatus.Equals(int.Parse(request.Status)))
                      .WhereIf(request.PaymentStartDate != null, r => r.PayTime > request.PaymentStartDate)
                      .WhereIf(request.PaymentEndDate != null, r => r.PayTime < Convert.ToDateTime(request.PaymentEndDate).AddDays(1))
                      ;
            if (CategoryList != null && CategoryList.Where(c => c.Equals("All")).Count() >= 1)
            {
                ReimburseInfos = ReimburseInfos.Where(r => CategoryList.Contains(r.ServiceRelations));
            }
            else
            {
                ReimburseInfos = ReimburseInfos.Where(r => r.ServiceRelations.Equals(loginContext.User.ServiceRelations));
            }
            if (!string.IsNullOrWhiteSpace(request.RemburseStatus))
            {
                switch (request.RemburseStatus)
                {
                    case "1":
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 1 || r.RemburseStatus == 2);
                        break;
                    case "3":
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 3);
                        break;
                    case "4":
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus >= 4 && r.RemburseStatus < 9);
                        break;
                    case "9":
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 9);
                        break;
                    case "0"://费用归属用
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus > 4 && r.RemburseStatus <= 9);
                        break;

                }
            }
            if (!loginContext.Roles.Any(r => r.Name.Equals("呼叫中心-查看")) && request.PageType == 1 && !loginContext.Roles.Any(r => r.Name.Equals("客服主管")) && loginContext.User.Account != Define.SYSTEM_USERNAME)
            {
                var orgRole = await UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.Key == Define.ORGROLE && c.FirstId == loginContext.User.Id).FirstOrDefaultAsync();
                if (orgRole != null)//查看本部下数据
                {
                    var orgId = orgRole.SecondId;
                    var userIds = await UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.SecondId == orgId && c.Key == Define.USERORG).Select(c => c.FirstId).ToListAsync();
                    ReimburseInfos = ReimburseInfos.Where(r => userIds.Contains(r.CreateUserId));
                }
                else
                {
                    ReimburseInfos = ReimburseInfos.Where(r => r.CreateUserId.Equals(loginContext.User.Id));
                }
            };
            #endregion

            #region 页面选择
            switch (request.PageType)
            {
                case 2:
                    List<int> Condition = new List<int>();
                    if (loginContext.Roles.Any(r => r.Name.Equals("客服主管")))
                    {
                        Condition.Add(4);
                        //ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 4);
                    }
                    if (loginContext.Roles.Any(r => r.Name.Equals("财务初审")))
                    {
                        Condition.Add(5);
                        //ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 5);
                    }
                    if (loginContext.Roles.Any(r => r.Name.Equals("财务复审")))
                    {
                        Condition.Add(6);
                        //ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 6);
                    }
                    if (loginContext.Roles.Any(r => r.Name.Equals("总经理")))
                    {
                        Condition.Add(7);
                        //ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 7);
                    }
                    if (Condition.Count > 0)
                    {
                        ReimburseInfos = ReimburseInfos.Where(r => Condition.Contains(r.RemburseStatus));
                    }
                    else
                    {
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 0);
                    }
                    break;
                case 3:
                    if (loginContext.Roles.Any(r => r.Name.Equals("客服主管")))
                    {
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus > 4);
                    }
                    else if (loginContext.Roles.Any(r => r.Name.Equals("财务初审")))
                    {
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus > 5);
                    }
                    else if (loginContext.Roles.Any(r => r.Name.Equals("财务复审")))
                    {
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus > 6);
                    }
                    else if (loginContext.Roles.Any(r => r.Name.Equals("总经理")))
                    {
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus > 7);
                    }
                    else
                    {
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 0);
                    }
                    break;
                case 4:
                    if (loginContext.Roles.Any(r => r.Name.Equals("客服主管")))
                    {
                        var eohids = await UnitWork.Find<ReimurseOperationHistory>(r => r.ApprovalStage >= 4 && r.ApprovalResult == "驳回").Select(r => r.ReimburseInfoId).Distinct().ToListAsync();

                        ReimburseInfos = ReimburseInfos.Where(r => eohids.Contains(r.Id) && r.RemburseStatus == 2);
                    }
                    else if (loginContext.Roles.Any(r => r.Name.Equals("财务初审")))
                    {
                        var eohids = await UnitWork.Find<ReimurseOperationHistory>(r => r.ApprovalStage >= 5 && r.ApprovalResult == "驳回").Select(r => r.ReimburseInfoId).Distinct().ToListAsync();

                        ReimburseInfos = ReimburseInfos.Where(r => eohids.Contains(r.Id) && r.RemburseStatus == 2);
                    }
                    else if (loginContext.Roles.Any(r => r.Name.Equals("财务复审")))
                    {
                        var eohids = await UnitWork.Find<ReimurseOperationHistory>(r => r.ApprovalStage >= 6 && r.ApprovalResult == "驳回").Select(r => r.ReimburseInfoId).Distinct().ToListAsync();

                        ReimburseInfos = ReimburseInfos.Where(r => eohids.Contains(r.Id) && r.RemburseStatus == 2);
                    }
                    else if (loginContext.Roles.Any(r => r.Name.Equals("总经理")))
                    {
                        var eohids = await UnitWork.Find<ReimurseOperationHistory>(r => r.ApprovalStage >= 7 && r.ApprovalResult == "驳回").Select(r => r.ReimburseInfoId).Distinct().ToListAsync();

                        ReimburseInfos = ReimburseInfos.Where(r => eohids.Contains(r.Id) && r.RemburseStatus == 2);
                    }
                    else
                    {
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 0);
                    }
                    break;
                case 5:
                    if (loginContext.Roles.Any(r => r.Name.Equals("出纳")) || loginContext.Roles.Any(r => r.Name.Equals("财务初审")) || loginContext.Roles.Any(r => r.Name.Equals("财务复审")))
                    {
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 8);
                    }
                    else
                    {
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 0);
                    }
                    break;
                case 6:
                    if (loginContext.Roles.Any(r => r.Name.Equals("出纳")))
                    {
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 9);
                    }
                    else
                    {
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 0);
                    }
                    break;
            }
            #endregion

            ReimburseInfos = ReimburseInfos.OrderByDescending(r => r.UpdateTime);
            if (request.PageType == 2)
            {
                ReimburseInfos = ReimburseInfos.OrderByDescending(r => r.UpdateTime);
            }
            else if (request.PageType == 5)
            {
                ReimburseInfos = ReimburseInfos.OrderBy(r => r.CreateUserId).ThenBy(r => r.MainId);
            }
            //ServiceOrderIds = ReimburseInfolist.Select(d => d.ServiceOrderId).ToList();
            var SelOrgName = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(null).Select(o => new { o.Id, o.Name, o.CascadeId }).ToListAsync();
            var SelUserName = await UnitWork.Find<User>(null).Select(u => new { u.Id, u.Name }).ToListAsync();
            var Relevances = await UnitWork.Find<Relevance>(r => r.Key == Define.USERORG).Select(r => new { r.FirstId, r.SecondId }).ToListAsync();
            var CompletionReports = await UnitWork.Find<CompletionReport>(c => c.ServiceMode == 1 && c.IsReimburse == 2).ToListAsync();
            var CompletionReportList = new List<CompletionReport>();
            if (request.CompletionStartDate != null || request.CompletionEndDate != null)
            {
                var CompletionReportGroupBy = CompletionReports.GroupBy(c => c.ServiceOrderId).Select(c => c).ToList();
                CompletionReportGroupBy = CompletionReportGroupBy.Where(c => c.Min(m => m.BusinessTripDate) > request.CompletionStartDate && c.Max(m => m.EndDate) <= Convert.ToDateTime(request.CompletionEndDate).AddDays(1)).ToList();
                ServiceOrderIds = CompletionReportGroupBy.Select(c => (int)c.Key).ToList();
                CompletionReportGroupBy.ForEach(f => CompletionReportList.AddRange(f.ToList()));
            }
            if (CompletionReportList.Count > 0)
            {
                CompletionReports = CompletionReportList;
            }
            ReimburseInfos = ReimburseInfos.WhereIf(ServiceOrderIds.Count > 0, r => ServiceOrderIds.Contains(r.ServiceOrderId));
            var ReimburseInfolist = await ReimburseInfos.Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();
            ServiceOrderIds = ReimburseInfolist.Select(d => d.ServiceOrderId).ToList();
            var ServiceOrders = await UnitWork.Find<ServiceOrder>(s => ServiceOrderIds.Contains(s.Id)).ToListAsync();
            var serviceDailyReports = await UnitWork.Find<ServiceDailyReport>(s => ServiceOrderIds.Contains((int)s.ServiceOrderId)).ToListAsync();
            var workbench = await UnitWork.Find<WorkbenchPending>(c => c.OrderType == 4).Select(c => new { c.ApprovalNumber, c.SourceNumbers }).ToListAsync();
            var ReimburseResps = from a in ReimburseInfolist
                                 join b in CompletionReports on a.ServiceOrderId equals b.ServiceOrderId
                                 join c in ServiceOrders on a.ServiceOrderId equals c.Id into ac
                                 from c in ac.DefaultIfEmpty()
                                 join d in SelUserName on a.CreateUserId equals d.Id into ad
                                 from d in ad.DefaultIfEmpty()
                                 join e in Relevances on a.CreateUserId equals e.FirstId into ae
                                 from e in ae.DefaultIfEmpty()
                                 join f in SelOrgName on e.SecondId equals f.Id into ef
                                 from f in ef.DefaultIfEmpty()
                                 select new { a, b, c, d, f };

            ReimburseResps = ReimburseResps.OrderByDescending(r => r.f.CascadeId).ToList();
            ReimburseResps = ReimburseResps.GroupBy(r => r.a.Id).Select(r => r.First()).OrderByDescending(r => r.a.UpdateTime).ToList();

            if (request.PageType == 2)
            {
                ReimburseResps = ReimburseResps.OrderByDescending(r => r.a.UpdateTime);
            }
            else if (request.PageType == 5)
            {
                ReimburseResps = ReimburseResps.OrderBy(r => r.a.CreateUserId).ThenBy(r => r.a.MainId);
            }
            result.Count = ReimburseInfos.Count();
            var ReimburseRespList = ReimburseResps.Select(r => new
            {
                ReimburseResp = r.a,
                fillTime = r.a.CreateTime.ToString("yyyy.MM.dd HH:mm:ss"),
                r.b.TerminalCustomerId,
                r.b.TerminalCustomer,
                BusinessTripDate = serviceDailyReports.Where(s => s.ServiceOrderId == r.a.ServiceOrderId).FirstOrDefault() == null ? CompletionReports.Where(c => c.ServiceOrderId.Equals(r.a.ServiceOrderId)).Min(c => c.BusinessTripDate) == null ? Convert.ToDateTime(CompletionReports.Where(c => c.ServiceOrderId.Equals(r.a.ServiceOrderId)).Max(c => c.CreateTime)).ToString("yyyy.MM.dd HH:mm:ss") : Convert.ToDateTime(CompletionReports.Where(c => c.ServiceOrderId.Equals(r.a.ServiceOrderId)).Min(c => c.BusinessTripDate)).ToString("yyyy.MM.dd HH:mm:ss") : Convert.ToDateTime(serviceDailyReports.Where(s => s.ServiceOrderId == r.a.ServiceOrderId).Min(s => s.EditTime)).ToString("yyyy.MM.dd HH:mm:ss"),
                EndDate = serviceDailyReports.Where(s => s.ServiceOrderId == r.a.ServiceOrderId).FirstOrDefault() == null ? CompletionReports.Where(c => c.ServiceOrderId.Equals(r.a.ServiceOrderId)).Max(c => c.EndDate) == null ? Convert.ToDateTime(CompletionReports.Where(c => c.ServiceOrderId.Equals(r.a.ServiceOrderId)).Max(c => c.CreateTime)).ToString("yyyy.MM.dd HH:mm:ss") : Convert.ToDateTime(CompletionReports.Where(c => c.ServiceOrderId.Equals(r.a.ServiceOrderId)).Max(c => c.EndDate)).ToString("yyyy.MM.dd HH:mm:ss") : Convert.ToDateTime(serviceDailyReports.Where(s => s.ServiceOrderId == r.a.ServiceOrderId).Max(s => s.EditTime)).ToString("yyyy.MM.dd HH:mm:ss"),
                Days = r.a.ReimburseTravellingAllowances?.Sum(r => r.Days),
                r.b.FromTheme,
                r.c.SalesMan,
                ApprovalNumber = workbench.Where(c => c.SourceNumbers == r.a.MainId).FirstOrDefault()?.ApprovalNumber,
                UserName = r.f.Name == null ? r.d.Name : r.f.Name + "-" + r.d.Name,
                //OrgName = r.f.Name,
                UpdateTime = r.a.ReimurseOperationHistories.OrderByDescending(r => r.CreateTime).FirstOrDefault() != null ? Convert.ToDateTime(r.a.ReimurseOperationHistories.OrderByDescending(r => r.CreateTime).FirstOrDefault()?.CreateTime).ToString("yyyy.MM.dd HH:mm:ss") : Convert.ToDateTime(r.a.UpdateTime).ToString("yyyy.MM.dd HH:mm:ss")
            }).OrderByDescending(r => r.UpdateTime).ToList();
            result.Data = ReimburseRespList;
            return result;
        }
        /// <summary>
        /// 获取总金额待支付已支付金额
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> GetMoney(QueryReimburseInfoListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            List<string> UserIds = new List<string>();
            if (!string.IsNullOrWhiteSpace(request.CreateUserName))
            {
                UserIds.AddRange(await UnitWork.Find<User>(u => u.Name.Contains(request.CreateUserName)).Select(u => u.Id).ToListAsync());
            }
            var result = new TableData();
            var reimburseInfos = UnitWork.Find<ReimburseInfo>(null).WhereIf(!string.IsNullOrWhiteSpace(request.CreateUserName), r => UserIds.Contains(r.CreateUserId));
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

            var currsoids = currentUser.Count > 0 ? await UnitWork.Find<ServiceWorkOrder>(c => currentUser.Contains(c.CurrentUserNsapId)).Select(c => c.ServiceOrderId).Distinct().ToListAsync() : null;
            var reimburseInfoList = await reimburseInfos.Select(r => new { r.RemburseStatus, r.TotalMoney, r.ServiceOrderId }).ToListAsync();
            var serverOrderIds = reimburseInfoList.Select(r => r.ServiceOrderId).ToList();
            var expends = await UnitWork.Find<ServiceDailyExpends>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(request.CreateUserName), r => UserIds.Contains(r.CreateUserId))
                .WhereIf(currsoids != null, r => currsoids.Contains(r.ServiceOrderId))//不能查看全部的则查看自己或部门下的服务单关联的日费
                .WhereIf(currsoids == null, s => !serverOrderIds.Contains(s.ServiceOrderId))//全部日费
                .SumAsync(s => s.TotalMoney);
            var totalmoney = reimburseInfoList.Sum(r => r.TotalMoney) + expends;
            var havepaid = reimburseInfoList.Where(r => r.RemburseStatus == 9).Sum(r => r.TotalMoney);
            var unpaid = reimburseInfoList.Where(r => r.RemburseStatus < 9 && r.RemburseStatus > 3).Sum(r => r.TotalMoney);
            var notsubmit = reimburseInfoList.Where(r => r.RemburseStatus <= 3).Sum(r => r.TotalMoney) + expends;
            result.Data = new { totalmoney, havepaid, unpaid, notsubmit };
            return result;
        }

        /// <summary>
        /// App加载列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <returns></returns>
        public async Task<TableData> AppLoad(QueryReimburseInfoListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(request.AppId));
            }

            #region 查询条件
            var result = new TableData();
            var ReimburseInfos = UnitWork.Find<ReimburseInfo>(r => r.CreateUserId.Equals(loginUser.Id));

            if (!string.IsNullOrWhiteSpace(request.RemburseStatus))
            {
                switch (request.RemburseStatus)
                {
                    case "1":
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus >= 2 && r.RemburseStatus < 9 && r.RemburseStatus != 3);
                        break;
                    case "3":
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 3 || r.RemburseStatus == 1);
                        break;
                    case "9":
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 9);
                        break;
                }
            }
            #endregion


            var ReimburseInfolist = await ReimburseInfos.OrderByDescending(r => r.CreateTime).Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();
            var SelOrgName = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(null).Select(o => new { o.Id, o.Name, o.CascadeId }).ToListAsync();
            var SelUserName = await UnitWork.Find<User>(null).Select(u => new { u.Id, u.Name }).ToListAsync();
            var Relevances = await UnitWork.Find<Relevance>(r => r.Key == Define.USERORG).Select(r => new { r.FirstId, r.SecondId }).ToListAsync();
            var rohs = await UnitWork.Find<ReimurseOperationHistory>(r => r.ApprovalResult == "驳回").ToListAsync();
            var ServiceOrderIds = ReimburseInfolist.Select(d => d.ServiceOrderId).ToList();
            var CompletionReports = await UnitWork.Find<CompletionReport>(c => ServiceOrderIds.Contains((int)c.ServiceOrderId)).ToListAsync();
            var ServiceOrders = await UnitWork.Find<ServiceOrder>(s => ServiceOrderIds.Contains(s.Id)).ToListAsync();

            var ReimburseResps = from a in ReimburseInfolist
                                 join b in CompletionReports on a.ServiceOrderId equals b.ServiceOrderId into ab
                                 from b in ab.DefaultIfEmpty()
                                 join c in ServiceOrders on a.ServiceOrderId equals c.Id into ac
                                 from c in ac.DefaultIfEmpty()
                                 join d in SelUserName on a.CreateUserId equals d.Id into ad
                                 from d in ad.DefaultIfEmpty()
                                 join e in Relevances on a.CreateUserId equals e.FirstId into ae
                                 from e in ae.DefaultIfEmpty()
                                 join f in SelOrgName on e.SecondId equals f.Id into ef
                                 from f in ef.DefaultIfEmpty()
                                 select new { a, b, c, d, f };

            ReimburseResps = ReimburseResps.OrderByDescending(r => r.f.CascadeId).ToList();
            ReimburseResps = ReimburseResps.GroupBy(r => r.a.Id).Select(r => r.First()).ToList();

            result.Count = ReimburseInfos.Count();
            result.Data = ReimburseResps.OrderByDescending(r => r.a.CreateTime).Select(r => new
            {
                ReimburseResp = r.a,
                RejectRemark = rohs.Where(o => o.ReimburseInfoId.Equals(r.a.Id)).OrderByDescending(o => o.CreateTime).Select(o => o.Remark).FirstOrDefault(),
                r.b.TerminalCustomerId,
                r.b.TerminalCustomer,
                r.b.BusinessTripDate,
                r.b.EndDate,
                r.b.BusinessTripDays,
                r.b.FromTheme,
                r.c.SalesMan,
                UserName = r.d.Name,
                OrgName = r.f.Name
            }).ToList();

            return result;
        }

        /// <summary>
        /// 获取未报销服务单列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> GetServiceOrder(QueryReimburseServerOrderListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(request.AppId));
            }

            var orgids = await UnitWork.Find<Relevance>(r => r.Key == Define.USERORG && r.FirstId == loginUser.Id).Select(r => r.SecondId).ToListAsync();
            var orgname = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(o => orgids.Contains(o.Id)).OrderByDescending(o => o.CascadeId).Select(o => o.Name).FirstOrDefaultAsync();
            var CompletionReports = await UnitWork.Find<CompletionReport>(c => c.CreateUserId.Equals(loginUser.Id) && c.IsReimburse < 2).OrderByDescending(c => c.CreateTime).ToListAsync();

            var ServiceOrderids = CompletionReports.Select(c => c.ServiceOrderId).Distinct().ToList();
            var ServiceOrders = await UnitWork.Find<ServiceOrder>(s => ServiceOrderids.Contains(s.Id) && (s.VestInOrg == 1 || s.VestInOrg == 3)).Include(s => s.ServiceWorkOrders).ToListAsync();
            List<ServiceOrder> ServiceOrderList = new List<ServiceOrder>();
            foreach (var item in ServiceOrders)
            {
                var WorkOrders = item.ServiceWorkOrders.Where(s => s.CurrentUserNsapId == loginUser.Id && (s.ServiceMode == 1 || item.VestInOrg == 3) && s.Status > 6).Count();
                var WorkOrderCount = item.ServiceWorkOrders.Where(s => s.CurrentUserNsapId == loginUser.Id && s.Status <= 6).Count();
                if (WorkOrders > 0 && WorkOrderCount <= 0)
                {
                    ServiceOrderList.Add(item);
                }
            }
            //判断是否有转派的单子
            var redeployList = await UnitWork.Find<ServiceRedeploy>(w => w.TechnicianId == request.AppId).ToListAsync();
            if (redeployList != null)
            {
                var redeployIds = redeployList.Select(s => s.ServiceOrderId).Distinct().ToList();
                var redeployOrderList = await UnitWork.Find<ServiceOrder>(s => redeployIds.Contains(s.Id)).Include(s => s.ServiceWorkOrders).ToListAsync();
                foreach (var item in redeployOrderList)
                {
                    ServiceOrderList.Add(item);
                }
            }
            var ServiceOrderLists = from a in ServiceOrderList
                                    join b in CompletionReports on a.Id equals b.ServiceOrderId
                                    where (b.ServiceMode == 1 || a.VestInOrg == 3)
                                    select new { a, b };


            ServiceOrderLists = ServiceOrderLists.GroupBy(r => r.a.Id).Select(g => g.First()).ToList();

            if (!string.IsNullOrWhiteSpace(request.SapId))
            {
                ServiceOrderLists = ServiceOrderLists.Where(s => s.a.U_SAP_ID.ToString().Contains(request.SapId) || s.b.TerminalCustomer.Contains(request.SapId)).ToList();
            }
            var result = new TableData();
            var objs = ServiceOrderLists.OrderBy(u => u.a.U_SAP_ID)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToList();
            result.Data = objs.Select(s => new
            {
                UserId = loginUser.Id,
                UserName = loginUser.Name,
                OrgName = orgname,
                ServiceRelations = loginUser.ServiceRelations == null ? "未录入" : loginUser.ServiceRelations,
                s.b.TerminalCustomer,
                s.b.TerminalCustomerId,
                s.a.Id,
                s.a.U_SAP_ID,
                s.b.FromTheme,
                s.b.Becity,
                s.b.Destination,
                BusinessTripDate = CompletionReports.Where(c => c.ServiceOrderId.Equals(s.a.Id) && c.ServiceMode == 1).Min(c => c.BusinessTripDate),
                EndDate = CompletionReports.Where(c => c.ServiceOrderId.Equals(s.a.Id) && c.ServiceMode == 1).Max(c => c.EndDate),
                MaterialCode = string.IsNullOrWhiteSpace(s.b.MaterialCode) ? "" : s.b.MaterialCode == "无序列号" ? "无序列号" : s.b.MaterialCode.Substring(0, s.b.MaterialCode.IndexOf("-"))
            }).ToList();
            result.Count = ServiceOrderLists.Count();
            return result;
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> GetUserDetails(QueryReimburseServerOrderListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(request.AppId));
            }
            decimal subsidies = 0;
            if (request.UserId != null)
            {
                loginUser.Id = request.UserId;
            }
            var orgids = await UnitWork.Find<Relevance>(r => r.Key == Define.USERORG && r.FirstId == loginUser.Id).Select(r => r.SecondId).ToListAsync();
            var orgname = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(o => orgids.Contains(o.Id)).OrderByDescending(o => o.CascadeId).Select(o => o.Name).ToListAsync();
            var categoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_TravellingAllowance")).Select(u => new { u.Name, u.DtValue, u.Description }).ToListAsync();
            categoryList = categoryList.Where(u => orgname.Contains(u.Name) || u.Description.Split(",").Contains(loginUser.Name)).ToList();
            if (categoryList != null && categoryList.Count() >= 1)
            {
                subsidies = Convert.ToDecimal(categoryList.FirstOrDefault().DtValue);
            }
            else
            {
                subsidies = 50;
            }

            #region
            //var orgname = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(o => orgids.Contains(o.Id)).OrderByDescending(o => o.CascadeId).Select(o => new { o.Name,o.CascadeId}).FirstOrDefaultAsync();
            //var CascadeId = orgname.CascadeId.ToString();
            //String[] split = CascadeId.Split(".");
            //int subsidies = 0;
            //if (split.Length > 3)
            //{
            //    var orgid = split[0] +"."+ split[1] + "." + split[2] + "." + split[3] + ".";
            //    var cascadeidname = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(o => o.CascadeId.Equals(orgid)).Select(o => new { o.Name, o.CascadeId }).FirstOrDefaultAsync();
            //    var idname = cascadeidname?.Name.Substring(0, 1);
            //    if (cascadeidname?.Name.Substring(0,1) == "R" || cascadeidname?.Name.Substring(0,1) == "M")
            //    {
            //        subsidies = 65;
            //    }
            //    else {
            //        subsidies = 50;
            //    }
            //}
            //else
            //{
            //    subsidies = 50;
            //}
            #endregion
            var result = new TableData();
            result.Data = new
            {
                Name = loginUser.Name,
                ServiceRelations = loginUser?.ServiceRelations == null ? "未录入" : loginUser.ServiceRelations,
                OrgName = orgname.FirstOrDefault(),
                Subsidies = subsidies,
            };
            return result;
        }

        /// <summary>
        /// 加载报销单详情
        /// </summary>
        /// <param name="ReimburseInfoId"></param>
        /// <returns></returns>
        public async Task<TableData> GetDetails(int ReimburseInfoId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            if (loginContext.Roles.Any(r => r.Name.Equals("客服主管")))
            {
                var obj = await UnitWork.Find<ReimburseInfo>(r => r.Id == ReimburseInfoId).FirstOrDefaultAsync();
                obj.IsRead = 2;
                await UnitWork.UpdateAsync<ReimburseInfo>(obj);
                await UnitWork.SaveAsync();
            }
            var result = new TableData();

            var Reimburse = await UnitWork.Find<ReimburseInfo>(r => r.Id == ReimburseInfoId)
                        //.Include(r => r.ReimburseAttachments)
                        .Include(r => r.ReimburseTravellingAllowances)
                        .Include(r => r.ReimburseFares)
                        .Include(r => r.ReimburseAccommodationSubsidies)
                        .Include(r => r.ReimburseOtherCharges)
                        //.Include(r => r.ReimurseOperationHistories)
                        .FirstOrDefaultAsync();
            Reimburse.ReimurseOperationHistories = await UnitWork.Find<ReimurseOperationHistory>(r => r.ReimburseInfoId == ReimburseInfoId).OrderBy(o => o.CreateTime).ToListAsync();
            #region 获取附件
            Reimburse.ReimburseAttachments = await UnitWork.Find<ReimburseAttachment>(r => r.ReimburseId == ReimburseInfoId && r.ReimburseType == 0).ToListAsync();
            var ReimburseResp = Reimburse.MapTo<ReimburseInfoResp>();
            List<string> fileids = Reimburse.ReimburseAttachments.Select(r => r.FileId).ToList();
            List<ReimburseAttachment> rffilemodel = new List<ReimburseAttachment>();
            List<ReimburseExpenseOrg> expenseOrg = new List<ReimburseExpenseOrg>();
            if (Reimburse.ReimburseTravellingAllowances != null && Reimburse.ReimburseTravellingAllowances.Count > 0)
            {
                var rtaids = ReimburseResp.ReimburseTravellingAllowances.Select(r => r.Id).ToList();
                expenseOrg.AddRange(await UnitWork.Find<ReimburseExpenseOrg>(r => rtaids.Contains(r.ExpenseId) && r.ExpenseType == 1).ToListAsync());
            }
            if (ReimburseResp.ReimburseFares != null && ReimburseResp.ReimburseFares.Count > 0)
            {
                var rfids = ReimburseResp.ReimburseFares.Select(r => r.Id).ToList();
                rffilemodel = await UnitWork.Find<ReimburseAttachment>(r => rfids.Contains(r.ReimburseId) && r.ReimburseType == 2).ToListAsync();
                expenseOrg.AddRange(await UnitWork.Find<ReimburseExpenseOrg>(r => rfids.Contains(r.ExpenseId) && r.ExpenseType == 2).ToListAsync());
            }
            if (ReimburseResp.ReimburseAccommodationSubsidies != null && ReimburseResp.ReimburseAccommodationSubsidies.Count > 0)
            {
                var rasids = ReimburseResp.ReimburseAccommodationSubsidies.Select(r => r.Id).ToList();
                rffilemodel.AddRange(await UnitWork.Find<ReimburseAttachment>(r => rasids.Contains(r.ReimburseId) && r.ReimburseType == 3).ToListAsync());
                expenseOrg.AddRange(await UnitWork.Find<ReimburseExpenseOrg>(r => rasids.Contains(r.ExpenseId) && r.ExpenseType == 3).ToListAsync());
            }
            if (ReimburseResp.ReimburseOtherCharges != null && ReimburseResp.ReimburseOtherCharges.Count > 0)
            {
                var rocids = ReimburseResp.ReimburseOtherCharges.Select(r => r.Id).ToList();
                rffilemodel.AddRange(await UnitWork.Find<ReimburseAttachment>(r => rocids.Contains(r.ReimburseId) && r.ReimburseType == 4).ToListAsync());
                expenseOrg.AddRange(await UnitWork.Find<ReimburseExpenseOrg>(r => rocids.Contains(r.ExpenseId) && r.ExpenseType == 4).ToListAsync());
            }
            fileids.AddRange(rffilemodel.Select(f => f.FileId).ToList());

            var file = await UnitWork.Find<UploadFile>(f => fileids.Contains(f.Id)).ToListAsync();

            ReimburseResp.ReimburseAttachments.ForEach(r => { r.AttachmentName = file.Where(f => f.Id.Equals(r.FileId)).Select(f => f.FileName).FirstOrDefault(); r.FileType = file.Where(f => f.Id.Equals(r.FileId)).Select(f => f.FileType).FirstOrDefault(); });
            if (Reimburse.ReimburseTravellingAllowances != null && Reimburse.ReimburseTravellingAllowances.Count > 0)
            {
                ReimburseResp.ReimburseTravellingAllowances.ForEach(r =>
                {
                    r.ExpenseType = "1";
                    r.ReimburseExpenseOrgs = (expenseOrg.Where(e => e.ExpenseId == r.Id && e.ExpenseType == 1).ToList()).MapToList<ReimburseExpenseOrgResp>();
                });
            }
            if (ReimburseResp.ReimburseFares != null && ReimburseResp.ReimburseFares.Count > 0)
            {
                ReimburseResp.ReimburseFares.ForEach(r =>
                {
                    r.ExpenseType = "2";
                    r.ReimburseAttachments = rffilemodel.Where(f => f.ReimburseId.Equals(r.Id) && f.ReimburseType == 2).Select(r => new ReimburseAttachmentResp
                    {
                        Id = r.Id,
                        FileId = r.FileId,
                        AttachmentName = file.FirstOrDefault(f => f.Id.Equals(r.FileId)).FileName,
                        FileType = file.FirstOrDefault(f => f.Id.Equals(r.FileId)).FileType,
                        ReimburseId = r.ReimburseId,
                        ReimburseType = r.ReimburseType,
                        AttachmentType = r.AttachmentType
                    }).ToList();
                    r.ReimburseExpenseOrgs = (expenseOrg.Where(e => e.ExpenseId == r.Id && e.ExpenseType == 2).ToList()).MapToList<ReimburseExpenseOrgResp>();
                });
            }
            if (ReimburseResp.ReimburseAccommodationSubsidies != null && ReimburseResp.ReimburseAccommodationSubsidies.Count > 0)
            {
                ReimburseResp.ReimburseAccommodationSubsidies.ForEach(r =>
                {
                    r.ExpenseType = "3";
                    r.ReimburseAttachments = rffilemodel.Where(f => f.ReimburseId.Equals(r.Id) && f.ReimburseType == 3).Select(r => new ReimburseAttachmentResp
                    {
                        Id = r.Id,
                        FileId = r.FileId,
                        AttachmentName = file.FirstOrDefault(f => f.Id.Equals(r.FileId)).FileName,
                        FileType = file.FirstOrDefault(f => f.Id.Equals(r.FileId)).FileType,
                        ReimburseId = r.ReimburseId,
                        ReimburseType = r.ReimburseType,
                        AttachmentType = r.AttachmentType
                    }).ToList();
                    r.ReimburseExpenseOrgs = (expenseOrg.Where(e => e.ExpenseId == r.Id && e.ExpenseType == 3).ToList()).MapToList<ReimburseExpenseOrgResp>();
                });
            }
            if (ReimburseResp.ReimburseOtherCharges != null && ReimburseResp.ReimburseOtherCharges.Count > 0)
            {
                ReimburseResp.ReimburseOtherCharges.ForEach(r =>
                {
                    r.ExpenseType = "4";
                    r.ReimburseAttachments = rffilemodel.Where(f => f.ReimburseId.Equals(r.Id) && f.ReimburseType == 4).Select(r => new ReimburseAttachmentResp
                    {
                        Id = r.Id,
                        FileId = r.FileId,
                        AttachmentName = file.FirstOrDefault(f => f.Id.Equals(r.FileId)).FileName,
                        FileType = file.FirstOrDefault(f => f.Id.Equals(r.FileId)).FileType,
                        ReimburseId = r.ReimburseId,
                        ReimburseType = r.ReimburseType,
                        AttachmentType = r.AttachmentType
                    }).ToList();
                    r.ReimburseExpenseOrgs = (expenseOrg.Where(e => e.ExpenseId == r.Id && e.ExpenseType == 4).ToList()).MapToList<ReimburseExpenseOrgResp>();
                });
            }

            #endregion

            var orgids = await UnitWork.Find<Relevance>(r => r.Key == Define.USERORG && r.FirstId == ReimburseResp.CreateUserId).Select(r => r.SecondId).ToListAsync();
            var orgname = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(o => orgids.Contains(o.Id)).OrderByDescending(o => o.CascadeId).Select(o => o.Name).FirstOrDefaultAsync();
            var serviceOrders = await UnitWork.Find<ServiceOrder>(s => s.Id == ReimburseResp.ServiceOrderId).Include(s => s.ServiceWorkOrders).FirstOrDefaultAsync();
            var quotationIds = await UnitWork.Find<Quotation>(q => q.ServiceOrderId == ReimburseResp.ServiceOrderId && q.CreateUserId.Equals(ReimburseResp.CreateUserId) && q.QuotationStatus == 11).Select(q => q.Id).ToListAsync();
            List<AddOrUpdateQuotationReq> quotations = new List<AddOrUpdateQuotationReq>();
            foreach (var item in quotationIds)
            {
                quotations.Add(await _quotation.GeneralDetails(item, null));
            }
            var CompletionReports = await UnitWork.Find<CompletionReport>(c => c.ServiceOrderId == ReimburseResp.ServiceOrderId && c.CreateUserId.Equals(ReimburseResp.CreateUserId) && c.ServiceMode == 1).ToListAsync();
            var completionreport = CompletionReports.FirstOrDefault();
            var ocrds = await UnitWork.Find<crm_ocrd>(o => serviceOrders.TerminalCustomerId.Equals(o.CardCode)).FirstOrDefaultAsync();
            result.Data = new
            {
                ReimburseResp = ReimburseResp,
                UserName = await UnitWork.Find<User>(u => u.Id.Equals(ReimburseResp.CreateUserId)).Select(u => u.Name).FirstOrDefaultAsync(),
                OrgName = orgname,
                Balance = ocrds?.Balance,
                //TerminalCustomer = completionreport.TerminalCustomer,
                //TerminalCustomerId = completionreport.TerminalCustomerId,
                FromTheme = completionreport.FromTheme,
                Becity = completionreport.Becity,
                //CompleteAddress = ServiceOrders.Province + ServiceOrders.City + ServiceOrders.Area + ServiceOrders.Addr,
                Destination = completionreport.Destination,
                //BusinessTripDate = CompletionReports.Min(c => c.BusinessTripDate),
                //EndDate = CompletionReports.Max(c => c.EndDate),
                MaterialCode = string.IsNullOrWhiteSpace(completionreport.MaterialCode) ? "" : completionreport.MaterialCode == "无序列号" ? "无序列号" : completionreport.MaterialCode.Substring(0, completionreport.MaterialCode.IndexOf("-")),
                ServiceOrders = serviceOrders,
                Quotations = quotations,
            };

            return result;
        }

        /// <summary>
        /// 保存报销单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public void Add(AddOrUpdateReimburseInfoReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = GetUserId(Convert.ToInt32(req.AppId)).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            #region 报销单唯一
            var ReimburseCount = UnitWork.Find<ReimburseInfo>(r => r.ServiceOrderId.Equals(req.ServiceOrderId) && r.CreateUserId.Equals(loginUser.Id)).Count();
            if (ReimburseCount > 0)
            {
                throw new CommonException("该服务单已提交报销单，不可二次使用！", Define.INVALID_ReimburseAgain);
            }
            #endregion
            var obj = Condition(req);
            obj.ReimburseTravellingAllowances.ForEach(r => r.CreateTime = Convert.ToDateTime(r.CreateTime));
            obj.ReimburseOtherCharges.ForEach(r => r.CreateTime = DateTime.Now);
            obj.ReimburseFares.ForEach(r => r.CreateTime = DateTime.Now);
            obj.ReimburseAccommodationSubsidies.ForEach(r => r.CreateTime = DateTime.Now);

            var dbContext = UnitWork.GetDbContext<ReimburseInfo>();
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    obj.UpdateTime = DateTime.Now;
                    obj.CreateTime = DateTime.Now;
                    obj.CreateUserId = loginUser.Id;
                    obj.IsRead = 1;
                    //判断是否存为草稿
                    if (!obj.IsDraft)
                    {
                        var maxmainid = UnitWork.Find<ReimburseInfo>(null).OrderByDescending(r => r.MainId).Select(r => r.MainId).FirstOrDefault();
                        //创建报销流程
                        var mf = _moduleFlowSchemeApp.Get(m => m.Module.Name.Equals("报销"));
                        var afir = new AddFlowInstanceReq();
                        afir.SchemeId = mf.FlowSchemeId;
                        afir.FrmType = 2;
                        afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                        afir.CustomName = $"报销" + DateTime.Now;
                        afir.FrmData = "{\"ReimburseInfoId\":\"" + obj.Id + "\"}";
                        obj.FlowInstanceId = _flowInstanceApp.CreateInstanceAndGetIdAsync(afir).ConfigureAwait(false).GetAwaiter().GetResult();
                        //添加报销单
                        obj.MainId = maxmainid + 1;
                        obj.RemburseStatus = 4;
                        obj = UnitWork.Add<ReimburseInfo, int>(obj);
                        UnitWork.Save();
                        //记录操作日志
                        UnitWork.Add<ReimurseOperationHistory>(new ReimurseOperationHistory
                        {
                            Action = "提交报销单",
                            CreateUser = loginUser.Name,
                            CreateUserId = loginUser.Id,
                            CreateTime = DateTime.Now,
                            ReimburseInfoId = obj.Id
                        });
                        var serviceOrederObj = UnitWork.Find<ServiceOrder>(s => s.Id == obj.ServiceOrderId).FirstOrDefault();
                        _workbenchApp.AddOrUpdate(new WorkbenchPending
                        {
                            OrderType = 4,
                            TerminalCustomer = serviceOrederObj.TerminalCustomer,
                            TerminalCustomerId = serviceOrederObj.TerminalCustomerId,
                            ServiceOrderId = serviceOrederObj.Id,
                            ServiceOrderSapId = (int)serviceOrederObj.U_SAP_ID,
                            UpdateTime = obj.UpdateTime,
                            Remark = obj.Remark,
                            FlowInstanceId = obj.FlowInstanceId,
                            TotalMoney = obj.TotalMoney,
                            Petitioner = loginUser.Name,
                            SourceNumbers = obj.MainId,
                            PetitionerId = loginUser.Id
                        }).ConfigureAwait(false).GetAwaiter().GetResult();
                        UnitWork.Save();
                    }
                    else
                    {
                        //添加报销单草稿
                        obj.RemburseStatus = 3;
                        obj = UnitWork.Add<ReimburseInfo, int>(obj);
                        UnitWork.Save();
                    }
                    //反写完工报告
                    UnitWork.Update<CompletionReport>(c => c.ServiceOrderId == obj.ServiceOrderId && c.CreateUserId == obj.CreateUserId, c => new CompletionReport { IsReimburse = 2 });
                    UnitWork.Save();
                    //保存附件
                    List<ReimburseAttachment> AttachmentList = new List<ReimburseAttachment>();
                    var Attachments = req.ReimburseAttachments.Where(r => r.IsAdd == true || r.IsAdd == null).MapToList<ReimburseAttachment>();
                    if (Attachments != null && Attachments.Count > 0)
                    {
                        Attachments.ForEach(f => { f.ReimburseId = obj.Id; f.ReimburseType = 0; f.Id = Guid.NewGuid().ToString(); });
                        AttachmentList.AddRange(Attachments);
                        //if (filemodel.Count > 0) UnitWork.BatchAdd<ReimburseAttachment>(filemodel.ToArray());
                    }

                    var rac = UnitWork.Find<ReimburseFare>(r => r.ReimburseInfoId == obj.Id).ToList();
                    foreach (var item in rac)
                    {
                        var racreq = req.ReimburseFares.Where(r => r.SerialNumber == item.SerialNumber).Select(r => r.ReimburseAttachments).FirstOrDefault();
                        if (racreq != null && racreq.Count > 0)
                        {
                            Attachments = racreq.Where(r => r.IsAdd == true || r.IsAdd == null).MapToList<ReimburseAttachment>();
                            Attachments.ForEach(f => { f.ReimburseId = item.Id; f.ReimburseType = 2; f.Id = Guid.NewGuid().ToString(); });
                            AttachmentList.AddRange(Attachments);
                            //if (filemodel.Count > 0) UnitWork.BatchAdd<ReimburseAttachment>(filemodel.ToArray());
                        }
                    }

                    var ras = UnitWork.Find<ReimburseAccommodationSubsidy>(r => r.ReimburseInfoId == obj.Id).ToList();
                    foreach (var item in ras)
                    {
                        var rasreq = req.ReimburseAccommodationSubsidies.Where(r => r.SerialNumber == item.SerialNumber).Select(r => r.ReimburseAttachments).FirstOrDefault();
                        if (rasreq != null && rasreq.Count > 0)
                        {
                            Attachments = rasreq.Where(r => r.IsAdd == true || r.IsAdd == null).MapToList<ReimburseAttachment>();
                            Attachments.ForEach(f => { f.ReimburseId = item.Id; f.ReimburseType = 3; f.Id = Guid.NewGuid().ToString(); });
                            AttachmentList.AddRange(Attachments);
                            //if (filemodel.Count > 0) UnitWork.BatchAdd<ReimburseAttachment>(filemodel.ToArray());
                        }
                    }

                    var roc = UnitWork.Find<ReimburseOtherCharges>(r => r.ReimburseInfoId == obj.Id).ToList();
                    foreach (var item in roc)
                    {
                        var rocreq = req.ReimburseOtherCharges.Where(r => r.SerialNumber == item.SerialNumber).Select(r => r.ReimburseAttachments).FirstOrDefault();
                        if (rocreq != null && rocreq.Count > 0)
                        {
                            Attachments = rocreq.Where(r => r.IsAdd == true || r.IsAdd == null).MapToList<ReimburseAttachment>();
                            Attachments.ForEach(f => { f.ReimburseId = item.Id; f.ReimburseType = 4; f.Id = Guid.NewGuid().ToString(); });
                            AttachmentList.AddRange(Attachments);
                            //if (filemodel.Count > 0) UnitWork.BatchAdd<ReimburseAttachment>(filemodel.ToArray());
                        }
                    }
                    if (AttachmentList != null && AttachmentList.Count > 0)
                    {
                        UnitWork.BatchAdd<ReimburseAttachment>(AttachmentList.ToArray());
                    }
                    UnitWork.Save();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw new Exception("添加报销单失败,请重试");
                }
            }
        }

        /// <summary>
        /// 新增出差补贴
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> AddTravellingAllowance(ReimburseTravellingAllowanceResp req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            if (await UnitWork.Find<ReimburseTravellingAllowance>(r => r.ReimburseInfoId == req.ReimburseInfoId && r.IsAdded == true).CountAsync() > 0)
            {
                throw new Exception("已新增出差补贴，不可多次新建");
            }
            var result = new TableData();
            if (req != null)
            {
                var travellingAllowances = req.MapTo<ReimburseTravellingAllowance>();
                travellingAllowances.CreateTime = DateTime.Now;
                travellingAllowances.IsAdded = true;
                travellingAllowances.ReimburseInfoId = (int)req.ReimburseInfoId;
                result.Data = await UnitWork.AddAsync<ReimburseTravellingAllowance, int>(travellingAllowances);
                var TotalMoney = travellingAllowances.Days * travellingAllowances.Money;
                await UnitWork.UpdateAsync<ReimburseInfo>(r => r.Id == req.ReimburseInfoId, r => new ReimburseInfo { TotalMoney = r.TotalMoney + TotalMoney });
                await UnitWork.SaveAsync();
            }
            return result;
        }

        /// <summary>
        /// 修改报销单 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public void UpDate(AddOrUpdateReimburseInfoReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = GetUserId(Convert.ToInt32(req.AppId)).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            var dbContext = UnitWork.GetDbContext<ReimburseInfo>();
            var obj = Condition(req);
            obj.IsRead = 1;
            using (var transaction = dbContext.Database.BeginTransactionAsync().ConfigureAwait(false).GetAwaiter().GetResult())
            {
                try
                {
                    //生成报销单号
                    if (!obj.IsDraft && string.IsNullOrWhiteSpace(req.FlowInstanceId) && (string.IsNullOrWhiteSpace(obj.MainId.ToString()) || obj.MainId == 0))
                    {
                        var maxmainid = UnitWork.Find<ReimburseInfo>(null).OrderByDescending(r => r.MainId).Select(r => r.MainId).FirstOrDefault();
                        obj.MainId = maxmainid + 1;
                    }

                    if (!req.IsDraft)
                    {
                        if (string.IsNullOrWhiteSpace(req.FlowInstanceId))
                        {
                            //添加流程
                            var mf = _moduleFlowSchemeApp.Get(m => m.Module.Name.Equals("报销"));
                            var afir = new AddFlowInstanceReq();
                            afir.SchemeId = mf.FlowSchemeId;
                            afir.FrmType = 2;
                            afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                            afir.CustomName = $"报销";
                            afir.FrmData = "{\"ReimburseInfoId\":\"" + obj.Id + "\"}";
                            obj.FlowInstanceId = _flowInstanceApp.CreateInstanceAndGetIdAsync(afir).ConfigureAwait(false).GetAwaiter().GetResult();
                        }
                        else
                        {
                            _flowInstanceApp.Start(new StartFlowInstanceReq { FlowInstanceId = obj.FlowInstanceId }).ConfigureAwait(false).GetAwaiter().GetResult();
                        }
                        //修改报销单
                        obj.RemburseStatus = 4;
                        UnitWork.Update<ReimburseInfo>(r => r.Id == obj.Id, r => new ReimburseInfo
                        {
                            UpdateTime = DateTime.Now,
                            ShortCustomerName = obj.ShortCustomerName,
                            ReimburseType = obj.ReimburseType,
                            ProjectName = obj.ProjectName,
                            RemburseStatus = obj.RemburseStatus,
                            IsRead = obj.IsRead,
                            TotalMoney = obj.TotalMoney,
                            Remark = obj.Remark,
                            BearToPay = obj.BearToPay,
                            Responsibility = obj.Responsibility,
                            PayTime = obj.PayTime,
                            IsDraft = obj.IsDraft,
                            FlowInstanceId = obj.FlowInstanceId,
                            MainId = obj.MainId,

                        });
                        UnitWork.Save();
                        //添加操作日志
                        UnitWork.Add<ReimurseOperationHistory>(new ReimurseOperationHistory
                        {
                            Action = "提交报销单",
                            CreateUser = loginUser.Name,
                            CreateUserId = loginUser.Id,
                            CreateTime = DateTime.Now,
                            ReimburseInfoId = obj.Id
                        });
                        //增加全局待处理
                        var serviceOrederObj = UnitWork.Find<ServiceOrder>(s => s.Id == obj.ServiceOrderId).FirstOrDefault();
                        _workbenchApp.AddOrUpdate(new WorkbenchPending
                        {
                            OrderType = 4,
                            TerminalCustomer = serviceOrederObj.TerminalCustomer,
                            TerminalCustomerId = serviceOrederObj.TerminalCustomerId,
                            ServiceOrderId = serviceOrederObj.Id,
                            ServiceOrderSapId = (int)serviceOrederObj.U_SAP_ID,
                            UpdateTime = obj.UpdateTime,
                            Remark = obj.Remark,
                            FlowInstanceId = obj.FlowInstanceId,
                            TotalMoney = obj.TotalMoney,
                            Petitioner = loginUser.Name,
                            SourceNumbers = obj.MainId,
                            PetitionerId = loginUser.Id,
                        }).ConfigureAwait(false).GetAwaiter().GetResult();
                        UnitWork.Save();
                    }
                    else
                    {
                        //修改报销单
                        obj.RemburseStatus = 3;
                        UnitWork.Update<ReimburseInfo>(r => r.Id == obj.Id, r => new ReimburseInfo
                        {
                            UpdateTime = DateTime.Now,
                            ShortCustomerName = obj.ShortCustomerName,
                            ReimburseType = obj.ReimburseType,
                            ProjectName = obj.ProjectName,
                            RemburseStatus = obj.RemburseStatus,
                            IsRead = obj.IsRead,
                            TotalMoney = obj.TotalMoney,
                            Remark = obj.Remark,
                            BearToPay = obj.BearToPay,
                            Responsibility = obj.Responsibility,
                            PayTime = obj.PayTime,
                            IsDraft = obj.IsDraft,
                            FlowInstanceId = obj.FlowInstanceId,
                            MainId = obj.MainId
                        });
                        UnitWork.Save();
                    }
                    //反写完工报告
                    UnitWork.Update<CompletionReport>(c => c.ServiceOrderId == obj.ServiceOrderId && c.CreateUserId == obj.CreateUserId, c => new CompletionReport { IsReimburse = 2 });
                    UnitWork.Save();
                    #region 删除
                    List<ReimburseAttachment> ReimburseAttachments = new List<ReimburseAttachment>();
                    ReimburseAttachments.AddRange(UnitWork.Find<ReimburseAttachment>(r => r.ReimburseId.Equals(req.Id) && r.ReimburseType == 0).ToList());

                    var Reimburse = UnitWork.Find<ReimburseInfo>(r => r.Id.Equals(req.Id))
                                    .Include(r => r.ReimburseAccommodationSubsidies).Include(r => r.ReimburseFares)
                                    .Include(r => r.ReimburseOtherCharges).Include(r => r.ReimburseTravellingAllowances).FirstOrDefault();
                    //删除出差补贴
                    if (Reimburse.ReimburseTravellingAllowances != null && Reimburse.ReimburseTravellingAllowances.Count > 0)
                    {
                        UnitWork.BatchDelete<ReimburseTravellingAllowance>(Reimburse.ReimburseTravellingAllowances.ToArray());
                    }
                    //删除交通补贴及
                    if (Reimburse.ReimburseFares != null && Reimburse.ReimburseFares.Count > 0)
                    {
                        var delids = Reimburse.ReimburseFares.Select(r => r.Id).ToList();
                        ReimburseAttachments.AddRange(UnitWork.Find<ReimburseAttachment>(r => delids.Contains(r.ReimburseId) && r.ReimburseType == 2).ToList());
                        UnitWork.BatchDelete<ReimburseFare>(Reimburse.ReimburseFares.ToArray());
                    }
                    //删除住宿补贴
                    if (Reimburse.ReimburseAccommodationSubsidies != null && Reimburse.ReimburseAccommodationSubsidies.Count > 0)
                    {
                        var delids = Reimburse.ReimburseAccommodationSubsidies.Select(r => r.Id).ToList();
                        ReimburseAttachments.AddRange(UnitWork.Find<ReimburseAttachment>(r => delids.Contains(r.ReimburseId) && r.ReimburseType == 3).ToList());
                        UnitWork.BatchDelete<ReimburseAccommodationSubsidy>(Reimburse.ReimburseAccommodationSubsidies.ToArray());
                    }
                    //删除其他补贴

                    if (Reimburse.ReimburseOtherCharges != null && Reimburse.ReimburseOtherCharges.Count > 0)
                    {
                        var delids = Reimburse.ReimburseOtherCharges.Select(r => r.Id).ToList();
                        ReimburseAttachments.AddRange(UnitWork.Find<ReimburseAttachment>(r => delids.Contains(r.ReimburseId) && r.ReimburseType == 4).ToList());
                        UnitWork.BatchDelete<ReimburseOtherCharges>(Reimburse.ReimburseOtherCharges.ToArray());
                    }
                    if (ReimburseAttachments != null && ReimburseAttachments.Count > 0)
                    {
                        UnitWork.BatchDelete<ReimburseAttachment>(ReimburseAttachments.ToArray());
                    }

                    UnitWork.Save();
                    #endregion
                    #region 新增
                    var ReimburseMap = req.MapTo<ReimburseInfo>();

                    if (ReimburseMap.ReimburseTravellingAllowances != null && ReimburseMap.ReimburseTravellingAllowances.Count > 0)
                    {
                        ReimburseMap.ReimburseTravellingAllowances.ForEach(a => a.ReimburseInfoId = obj.Id);
                        UnitWork.BatchAdd<ReimburseTravellingAllowance, int>(ReimburseMap.ReimburseTravellingAllowances.ToArray());
                    }
                    if (ReimburseMap.ReimburseFares != null && ReimburseMap.ReimburseFares.Count > 0)
                    {
                        ReimburseMap.ReimburseFares.ForEach(a => a.ReimburseInfoId = obj.Id);
                        UnitWork.BatchAdd<ReimburseFare, int>(ReimburseMap.ReimburseFares.ToArray());
                    }
                    if (ReimburseMap.ReimburseAccommodationSubsidies != null && ReimburseMap.ReimburseAccommodationSubsidies.Count > 0)
                    {
                        ReimburseMap.ReimburseAccommodationSubsidies.ForEach(a => a.ReimburseInfoId = obj.Id);
                        UnitWork.BatchAdd<ReimburseAccommodationSubsidy, int>(ReimburseMap.ReimburseAccommodationSubsidies.ToArray());
                    }
                    if (ReimburseMap.ReimburseOtherCharges != null && ReimburseMap.ReimburseOtherCharges.Count > 0)
                    {
                        ReimburseMap.ReimburseOtherCharges.ForEach(a => a.ReimburseInfoId = obj.Id);
                        UnitWork.BatchAdd<ReimburseOtherCharges, int>(ReimburseMap.ReimburseOtherCharges.ToArray());
                    }

                    UnitWork.Save();
                    #endregion
                    #region 新增附件
                    List<ReimburseAttachment> AttachmentList = new List<ReimburseAttachment>();
                    var Attachments = req.ReimburseAttachments.Where(r => r.IsAdd == true || r.IsAdd == null).MapToList<ReimburseAttachment>();
                    if (Attachments != null && Attachments.Count > 0)
                    {
                        Attachments.ForEach(f => { f.ReimburseId = obj.Id; f.ReimburseType = 0; f.Id = Guid.NewGuid().ToString(); });
                        AttachmentList.AddRange(Attachments);
                        //if (filemodel.Count > 0) UnitWork.BatchAdd<ReimburseAttachment>(filemodel.ToArray());
                    }

                    var rac = UnitWork.Find<ReimburseFare>(r => r.ReimburseInfoId == obj.Id).ToList();
                    foreach (var item in rac)
                    {
                        var racreq = req.ReimburseFares.Where(r => r.SerialNumber == item.SerialNumber).Select(r => r.ReimburseAttachments).FirstOrDefault();
                        if (racreq != null && racreq.Count > 0)
                        {
                            Attachments = racreq.Where(r => r.IsAdd == true || r.IsAdd == null).MapToList<ReimburseAttachment>();
                            Attachments.ForEach(f => { f.ReimburseId = item.Id; f.ReimburseType = 2; f.Id = Guid.NewGuid().ToString(); });
                            AttachmentList.AddRange(Attachments);
                            //if (filemodel.Count > 0) UnitWork.BatchAdd<ReimburseAttachment>(filemodel.ToArray());
                        }
                    }

                    var ras = UnitWork.Find<ReimburseAccommodationSubsidy>(r => r.ReimburseInfoId == obj.Id).ToList();
                    foreach (var item in ras)
                    {
                        var rasreq = req.ReimburseAccommodationSubsidies.Where(r => r.SerialNumber == item.SerialNumber).Select(r => r.ReimburseAttachments).FirstOrDefault();
                        if (rasreq != null && rasreq.Count > 0)
                        {
                            Attachments = rasreq.Where(r => r.IsAdd == true || r.IsAdd == null).MapToList<ReimburseAttachment>();
                            Attachments.ForEach(f => { f.ReimburseId = item.Id; f.ReimburseType = 3; f.Id = Guid.NewGuid().ToString(); });
                            AttachmentList.AddRange(Attachments);
                            //if (filemodel.Count > 0) UnitWork.BatchAdd<ReimburseAttachment>(filemodel.ToArray());
                        }
                    }

                    var roc = UnitWork.Find<ReimburseOtherCharges>(r => r.ReimburseInfoId == obj.Id).ToList();
                    foreach (var item in roc)
                    {
                        var rocreq = req.ReimburseOtherCharges.Where(r => r.SerialNumber == item.SerialNumber).Select(r => r.ReimburseAttachments).FirstOrDefault();
                        if (rocreq != null && rocreq.Count > 0)
                        {
                            Attachments = rocreq.Where(r => r.IsAdd == true || r.IsAdd == null).MapToList<ReimburseAttachment>();
                            Attachments.ForEach(f => { f.ReimburseId = item.Id; f.ReimburseType = 4; f.Id = Guid.NewGuid().ToString(); });
                            AttachmentList.AddRange(Attachments);
                            //if (filemodel.Count > 0) UnitWork.BatchAdd<ReimburseAttachment>(filemodel.ToArray());
                        }
                    }
                    if (AttachmentList != null && AttachmentList.Count > 0)
                    {
                        UnitWork.BatchAdd<ReimburseAttachment>(AttachmentList.ToArray());
                    }
                    UnitWork.Save();
                    #endregion
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw new Exception("修改报销单失败,请重试。");
                }


            }


        }

        /// <summary>
        /// 删除报销单费用
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task DeleteCost(ReimburseRevocationReq req)
        {
            //1 出差补贴， 2 交通费用， 3住宿补贴， 4 其他费用
            decimal money = 0;
            List<ReimburseAttachment> ReimburseAttachments = new List<ReimburseAttachment>();
            switch (req.ReimburseType)
            {
                case 1:
                    var ReimburseTravellingAllowances = await UnitWork.Find<ReimburseTravellingAllowance>(r => r.Id.Equals(req.ReimburseCostId) && r.ReimburseInfoId.Equals(req.ReimburseInfoId)).FirstOrDefaultAsync();
                    money = Convert.ToDecimal(ReimburseTravellingAllowances.Days * ReimburseTravellingAllowances.Money);
                    ReimburseAttachments = await UnitWork.Find<ReimburseAttachment>(r => r.ReimburseId.Equals(ReimburseTravellingAllowances.Id) && r.ReimburseType == 1).ToListAsync();
                    await UnitWork.DeleteAsync<ReimburseTravellingAllowance>(ReimburseTravellingAllowances);
                    break;
                case 2:
                    var ReimburseFares = await UnitWork.Find<ReimburseFare>(r => r.Id.Equals(req.ReimburseCostId) && r.ReimburseInfoId.Equals(req.ReimburseInfoId)).FirstOrDefaultAsync();
                    money = Convert.ToDecimal(ReimburseFares.Money);
                    ReimburseAttachments = await UnitWork.Find<ReimburseAttachment>(r => r.ReimburseId.Equals(ReimburseFares.Id) && r.ReimburseType == 2).ToListAsync();
                    await UnitWork.DeleteAsync<ReimburseFare>(ReimburseFares);
                    break;
                case 3:
                    var ReimburseAccommodationSubsidies = await UnitWork.Find<ReimburseAccommodationSubsidy>(r => r.Id.Equals(req.ReimburseCostId) && r.ReimburseInfoId.Equals(req.ReimburseInfoId)).FirstOrDefaultAsync();
                    money = Convert.ToDecimal(ReimburseAccommodationSubsidies.TotalMoney);
                    ReimburseAttachments = await UnitWork.Find<ReimburseAttachment>(r => r.ReimburseId.Equals(ReimburseAccommodationSubsidies.Id) && r.ReimburseType == 3).ToListAsync();
                    await UnitWork.DeleteAsync<ReimburseAccommodationSubsidy>(ReimburseAccommodationSubsidies);
                    break;
                case 4:
                    var ReimburseOtherCharges = await UnitWork.Find<ReimburseOtherCharges>(r => r.Id.Equals(req.ReimburseCostId) && r.ReimburseInfoId.Equals(req.ReimburseInfoId)).FirstOrDefaultAsync();
                    money = Convert.ToDecimal(ReimburseOtherCharges.Money);
                    ReimburseAttachments = await UnitWork.Find<ReimburseAttachment>(r => r.ReimburseId.Equals(ReimburseOtherCharges.Id) && r.ReimburseType == 4).ToListAsync();
                    await UnitWork.DeleteAsync<ReimburseOtherCharges>(ReimburseOtherCharges);
                    break;
                default:
                    break;
            }
            await UnitWork.UpdateAsync<ReimburseInfo>(r => r.Id.Equals(req.ReimburseInfoId), r => new ReimburseInfo
            {
                TotalMoney = r.TotalMoney - money
            });
            if (ReimburseAttachments.Count > 0)
            {
                await UnitWork.BatchDeleteAsync<ReimburseAttachment>(ReimburseAttachments.ToArray());
            }
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 审批报销单 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Accraditation(AccraditationReimburseInfoReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            ReimurseOperationHistory eoh = new ReimurseOperationHistory();
            var obj = await UnitWork.Find<ReimburseInfo>(r => r.Id == req.Id).Include(r => r.ReimburseTravellingAllowances)
                .Include(r => r.ReimburseFares).Include(r => r.ReimburseAccommodationSubsidies).Include(r => r.ReimburseOtherCharges).FirstOrDefaultAsync();
            if (obj.RemburseStatus < 4)
            {
                throw new Exception("报销单已撤回，不可操作。");
            }
            //obj.ShortCustomerName = req.ShortCustomerName;
            //obj.ProjectName = req.ProjectName;
            //obj.BearToPay = req.BearToPay;
            //obj.ReimburseType = req.ReimburseType;
            //obj.Responsibility = req.Responsibility;
            obj.UpdateTime = DateTime.Now;
            eoh.ApprovalStage = obj.RemburseStatus;
            if (loginContext.Roles.Any(r => r.Name.Equals("客服主管")) && obj.RemburseStatus == 4)
            {
                if (!req.IsReject)
                {
                    if (req.travelOrgResults != null)
                    {
                        var travelExpendIds = req.travelOrgResults.Select(s => s.Id).ToList();
                        var reimburseTravellingAllowances = await UnitWork.Find<ReimburseTravellingAllowance>(w => travelExpendIds.Contains(w.Id)).ToListAsync();
                        foreach (var item in req.travelOrgResults)
                        {
                            var detail = reimburseTravellingAllowances.Where(w => w.Id == item.Id).FirstOrDefault();
                            detail.ExpenseOrg = item.Value;
                        }
                        await UnitWork.BatchUpdateAsync(reimburseTravellingAllowances.ToArray());
                    }
                    if (req.transportOrgResults != null)
                    {
                        var transportExpendIds = req.transportOrgResults.Select(s => s.Id).ToList();
                        var reimburseFares = await UnitWork.Find<ReimburseFare>(w => transportExpendIds.Contains(w.Id)).ToListAsync();
                        foreach (var item in req.transportOrgResults)
                        {
                            var detail = reimburseFares.Where(w => w.Id == item.Id).FirstOrDefault();
                            detail.ExpenseOrg = item.Value;
                        }
                        await UnitWork.BatchUpdateAsync(reimburseFares.ToArray());
                    }
                    if (req.hotelOrgResults != null)
                    {
                        var hotelExpendIds = req.hotelOrgResults.Select(s => s.Id).ToList();
                        var reimburseAccommodationSubsidies = await UnitWork.Find<ReimburseAccommodationSubsidy>(w => hotelExpendIds.Contains(w.Id)).ToListAsync();
                        foreach (var item in req.hotelOrgResults)
                        {
                            var detail = reimburseAccommodationSubsidies.Where(w => w.Id == item.Id).FirstOrDefault();
                            detail.ExpenseOrg = item.Value;
                        }
                        await UnitWork.BatchUpdateAsync(reimburseAccommodationSubsidies.ToArray());
                    }
                    if (req.otherOrgResults != null)
                    {
                        var otherExpendIds = req.otherOrgResults.Select(s => s.Id).ToList();
                        var reimburseOtherCharges = await UnitWork.Find<ReimburseOtherCharges>(w => otherExpendIds.Contains(w.Id)).ToListAsync();
                        foreach (var item in req.otherOrgResults)
                        {
                            var detail = reimburseOtherCharges.Where(w => w.Id == item.Id).FirstOrDefault();
                            detail.ExpenseOrg = item.Value;
                        }
                        await UnitWork.BatchUpdateAsync(reimburseOtherCharges.ToArray());
                    }
                    if (req.ReimburseExpenseOrgs != null && req.ReimburseExpenseOrgs.Count() > 0)
                    {
                        var reimburseExpenseOrgs = req.ReimburseExpenseOrgs.MapToList<ReimburseExpenseOrg>();
                        reimburseExpenseOrgs.ForEach(o =>
                        {
                            o.CreateTime = DateTime.Now; o.UpdateTime = DateTime.Now; o.ExpenseSatus = 1;
                            switch (o.ExpenseType)
                            {
                                case 1:
                                    var rta = obj.ReimburseTravellingAllowances.Where(r => r.Id == o.ExpenseId).FirstOrDefault();
                                    o.Money = (rta.Days * rta.Money) * (o.Ratio / 100);
                                    break;
                                case 2:
                                    var rf = obj.ReimburseFares.Where(r => r.Id == o.ExpenseId).FirstOrDefault();
                                    o.Money = rf.Money * (o.Ratio / 100);
                                    break;
                                case 3:
                                    var ras = obj.ReimburseAccommodationSubsidies.Where(r => r.Id == o.ExpenseId).FirstOrDefault();
                                    o.Money = ras.TotalMoney * (o.Ratio / 100);
                                    break;
                                case 4:
                                    var roc = obj.ReimburseOtherCharges.Where(r => r.Id == o.ExpenseId).FirstOrDefault();
                                    o.Money = roc.Money * (o.Ratio / 100);
                                    break;
                            }
                        });
                        await UnitWork.BatchAddAsync<ReimburseExpenseOrg>(reimburseExpenseOrgs.ToArray());
                    }
                }
                eoh.Action = "客服主管审批";
                obj.RemburseStatus = 5;
            }
            else if (loginContext.Roles.Any(r => r.Name.Equals("财务初审")) && obj.RemburseStatus == 5)
            {
                eoh.Action = "财务初审";
                obj.RemburseStatus = 6;
            }
            else if (loginContext.Roles.Any(r => r.Name.Equals("财务复审")) && obj.RemburseStatus == 6)
            {
                eoh.Action = "财务复审";
                obj.RemburseStatus = 7;
            }
            else if (loginContext.Roles.Any(r => r.Name.Equals("总经理")) && (obj.RemburseStatus == 7 || obj.RemburseStatus == 8))
            {
                eoh.Action = "总经理审批";
                obj.RemburseStatus = 8;
            }
            else if (loginContext.Roles.Any(r => r.Name.Equals("出纳")) && obj.RemburseStatus == 8)
            {
                eoh.Action = "已支付";
                eoh.ApprovalResult = "已支付";
                obj.PayTime = DateTime.Now;
                obj.RemburseStatus = 9;
            }
            else
            {
                throw new Exception("审批失败，暂无权限审批当前流程。");
            }
            VerificationReq VerificationReqModle = new VerificationReq
            {
                NodeRejectStep = "",
                NodeRejectType = "0",
                FlowInstanceId = obj.FlowInstanceId,
                VerificationFinally = "1",
                VerificationOpinion = "同意",
            };
            if (req.IsReject)
            {
                VerificationReqModle.VerificationFinally = "3";
                VerificationReqModle.VerificationOpinion = req.Remark;
                VerificationReqModle.NodeRejectType = "1";
                await _flowInstanceApp.Verification(VerificationReqModle);
                obj.RemburseStatus = 2;
                eoh.ApprovalResult = "驳回";
                var rtaIds = obj.ReimburseTravellingAllowances.Select(r => r.Id).ToList();
                var rfIds = obj.ReimburseFares.Select(r => r.Id).ToList();
                var rasIds = obj.ReimburseAccommodationSubsidies.Select(r => r.Id).ToList();
                var rocIds = obj.ReimburseOtherCharges.Select(r => r.Id).ToList();
                await UnitWork.DeleteAsync<ReimburseExpenseOrg>(r => (rtaIds.Contains(r.ExpenseId) && r.ExpenseType == 1) || (rfIds.Contains(r.ExpenseId) && r.ExpenseType == 2) || (rasIds.Contains(r.ExpenseId) && r.ExpenseType == 3) || (rocIds.Contains(r.ExpenseId) && r.ExpenseType == 4));
            }
            else
            {
                if (req.BearToPay == "2")
                {
                    eoh.Action = "已结束";
                    eoh.ApprovalResult = "部门承担";
                    obj.RemburseStatus = -1;
                    obj.FlowInstanceId = "";
                    List<string> ids = new List<string>();
                    ids.Add(obj.FlowInstanceId);
                    await _flowInstanceApp.DeleteAsync(ids.ToArray());
                }
                else
                {

                    eoh.ApprovalResult = "同意";
                    await _flowInstanceApp.Verification(VerificationReqModle);
                }
            }
            await UnitWork.UpdateAsync<ReimburseInfo>(obj);
            var seleoh = await UnitWork.Find<ReimurseOperationHistory>(r => r.ReimburseInfoId.Equals(obj.Id)).OrderByDescending(r => r.CreateTime).FirstOrDefaultAsync();
            eoh.CreateUser = loginContext.User.Name;
            eoh.CreateUserId = loginContext.User.Id;
            eoh.CreateTime = DateTime.Now;
            eoh.ReimburseInfoId = obj.Id;
            eoh.Remark = req.Remark;
            eoh.IntervalTime = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(seleoh.CreateTime)).TotalSeconds);
            await UnitWork.AddAsync<ReimurseOperationHistory>(eoh);
            //修改全局待处理
            await UnitWork.UpdateAsync<WorkbenchPending>(w => w.SourceNumbers == obj.MainId && w.OrderType == 4, w => new WorkbenchPending
            {
                UpdateTime = obj.UpdateTime,
            });
            await UnitWork.SaveAsync();

        }

        /// <summary>
        /// 设置费用归属
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task SetExpenseOrgs(AccraditationReimburseInfoReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var obj = await UnitWork.Find<ReimburseInfo>(r => r.Id == req.Id).Include(r => r.ReimburseTravellingAllowances)
                .Include(r => r.ReimburseFares).Include(r => r.ReimburseAccommodationSubsidies).Include(r => r.ReimburseOtherCharges).FirstOrDefaultAsync();
            List<ReimburseExpenseOrg> expenseOrgs = new List<ReimburseExpenseOrg>(); 
            if (req.ReimburseExpenseOrgs != null && req.ReimburseExpenseOrgs.Count() > 0)
            {
                //旧数据删除
                if (obj.ReimburseTravellingAllowances!=null && obj.ReimburseTravellingAllowances.Count>0)
                {
                    var ids = obj.ReimburseTravellingAllowances.Select(c => c.Id).ToList();
                    expenseOrgs.AddRange(await UnitWork.Find<ReimburseExpenseOrg>(r => ids.Contains(r.ExpenseId) && r.ExpenseType == 1).ToListAsync());
                }
                if (obj.ReimburseFares!=null && obj.ReimburseFares.Count>0)
                {
                    var rfids = obj.ReimburseFares.Select(r => r.Id).ToList();
                    expenseOrgs.AddRange(await UnitWork.Find<ReimburseExpenseOrg>(r => rfids.Contains(r.ExpenseId) && r.ExpenseType == 2).ToListAsync());
                }
                if (obj.ReimburseAccommodationSubsidies!=null && obj.ReimburseAccommodationSubsidies.Count>0)
                {
                    var rasids = obj.ReimburseAccommodationSubsidies.Select(r => r.Id).ToList();
                    expenseOrgs.AddRange(await UnitWork.Find<ReimburseExpenseOrg>(r => rasids.Contains(r.ExpenseId) && r.ExpenseType == 3).ToListAsync());
                }
                if (obj.ReimburseOtherCharges != null && obj.ReimburseOtherCharges.Count > 0)
                {
                    var rocids = obj.ReimburseOtherCharges.Select(r => r.Id).ToList();
                    expenseOrgs.AddRange(await UnitWork.Find<ReimburseExpenseOrg>(r => rocids.Contains(r.ExpenseId) && r.ExpenseType == 4).ToListAsync());
                }

                await UnitWork.BatchDeleteAsync(expenseOrgs.ToArray());

                
                var reimburseExpenseOrgs = req.ReimburseExpenseOrgs.MapToList<ReimburseExpenseOrg>();
                reimburseExpenseOrgs.ForEach(o =>
                {
                    o.CreateTime = DateTime.Now; o.UpdateTime = DateTime.Now; o.ExpenseSatus = 1;
                    switch (o.ExpenseType)
                    {
                        case 1:
                            var rta = obj.ReimburseTravellingAllowances.Where(r => r.Id == o.ExpenseId).FirstOrDefault();
                            o.Money = (rta.Days * rta.Money) * (o.Ratio / 100);
                            break;
                        case 2:
                            var rf = obj.ReimburseFares.Where(r => r.Id == o.ExpenseId).FirstOrDefault();
                            o.Money = rf.Money * (o.Ratio / 100);
                            break;
                        case 3:
                            var ras = obj.ReimburseAccommodationSubsidies.Where(r => r.Id == o.ExpenseId).FirstOrDefault();
                            o.Money = ras.TotalMoney * (o.Ratio / 100);
                            break;
                        case 4:
                            var roc = obj.ReimburseOtherCharges.Where(r => r.Id == o.ExpenseId).FirstOrDefault();
                            o.Money = roc.Money * (o.Ratio / 100);
                            break;
                    }
                });
                await UnitWork.BatchAddAsync<ReimburseExpenseOrg>(reimburseExpenseOrgs.ToArray());
                await UnitWork.SaveAsync();
            }
        }

        /// <summary>
        /// 部门主管审批报销单 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task SupervisorAccraditation(AccraditationReimburseInfoReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            ReimurseOperationHistory eoh = new ReimurseOperationHistory();

            var obj = await UnitWork.Find<ReimburseInfo>(r => r.Id == req.Id).FirstOrDefaultAsync();

            if (obj.RemburseStatus < 4 && obj.RemburseStatus > 9)
            {
                throw new Exception("当前报销单状态，不可操作。");
            }
            if (req.IsReject)
            {
                List<string> ids = new List<string>();
                ids.Add(obj.FlowInstanceId);
                await _flowInstanceApp.DeleteAsync(ids.ToArray());
                obj.RemburseStatus = 2;
                obj.FlowInstanceId = "";
                eoh.ApprovalResult = "驳回";
            }
            await UnitWork.UpdateAsync<ReimburseInfo>(obj);
            var seleoh = await UnitWork.Find<ReimurseOperationHistory>(r => r.ReimburseInfoId.Equals(obj.Id)).OrderByDescending(r => r.CreateTime).FirstOrDefaultAsync();
            eoh.CreateUser = loginContext.User.Name;
            eoh.CreateUserId = loginContext.User.Id;
            eoh.CreateTime = DateTime.Now;
            eoh.ReimburseInfoId = obj.Id;
            eoh.Remark = req.Remark;
            eoh.IntervalTime = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(seleoh.CreateTime)).TotalSeconds);
            await UnitWork.AddAsync<ReimurseOperationHistory>(eoh);
            await UnitWork.SaveAsync();

        }

        /// <summary>
        /// 批量审批报销单 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task BatchAccraditation(AccraditationReimburseInfoReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            ReimurseOperationHistory eoh = new ReimurseOperationHistory();

            var dbContext = UnitWork.GetDbContext<ReimburseInfo>();
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    List<ReimurseOperationHistory> history = new List<ReimurseOperationHistory>();
                    foreach (var item in req.ReimburseId)
                    {
                        var obj = await UnitWork.Find<ReimburseInfo>(r => r.Id == item).FirstOrDefaultAsync();

                        VerificationReq VerificationReqModle = new VerificationReq
                        {
                            NodeRejectStep = "",
                            NodeRejectType = "0",
                            FlowInstanceId = obj.FlowInstanceId,
                            VerificationFinally = "1",
                            VerificationOpinion = "同意",
                        };
                        if (loginContext.Roles.Any(r => r.Name.Equals("出纳")) && obj.RemburseStatus == 8)
                        {
                            eoh.Action = "已支付";
                            eoh.ApprovalResult = "已支付";
                            obj.RemburseStatus = 9;
                            obj.PayTime = DateTime.Now;
                            await _flowInstanceApp.Verification(VerificationReqModle);
                        }
                        obj.UpdateTime = DateTime.Now;
                        await UnitWork.UpdateAsync<ReimburseInfo>(obj);
                        var seleoh = await UnitWork.Find<ReimurseOperationHistory>(r => r.ReimburseInfoId.Equals(obj.Id)).OrderByDescending(r => r.CreateTime).FirstOrDefaultAsync();
                        eoh.CreateUser = loginContext.User.Name;
                        eoh.CreateUserId = loginContext.User.Id;
                        eoh.CreateTime = DateTime.Now;
                        eoh.ReimburseInfoId = obj.Id;
                        eoh.IntervalTime = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(seleoh.CreateTime)).TotalSeconds);
                        eoh.Id = Guid.NewGuid().ToString();
                        history.Add(eoh);
                    }
                    await UnitWork.BatchAddAsync<ReimurseOperationHistory>(history.ToArray());
                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("审批失败,请重试" + ex.Message);
                }
            }

        }

        /// <summary>
        /// 撤回报销单 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> Revocation(ReimburseRevocationReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(req.AppId));
            }
            var result = new TableData();
            var obj = await UnitWork.Find<ReimburseInfo>(r => r.Id.Equals(req.ReimburseInfoId) && r.CreateUserId.Equals(loginUser.Id)).FirstOrDefaultAsync();
            if (obj != null && obj.IsRead == 1)
            {
                await UnitWork.UpdateAsync<ReimburseInfo>(r => r.Id == obj.Id, r => new ReimburseInfo
                {
                    RemburseStatus = 1,
                    IsDraft = true,
                    IsRead = 1,
                    UpdateTime = DateTime.Now
                }); ;
                await UnitWork.AddAsync<ReimurseOperationHistory>(new ReimurseOperationHistory
                {
                    Action = "撤回报销单",
                    CreateUser = loginUser.Name,
                    CreateUserId = loginUser.Id,
                    CreateTime = DateTime.Now,
                    ReimburseInfoId = obj.Id
                });
                await _flowInstanceApp.ReCall(new RecallFlowInstanceReq { FlowInstanceId = obj.FlowInstanceId, Description = "撤回报销单" });
                await UnitWork.SaveAsync();
                result.Code = 200;
                result.Message = "已撤回到草稿箱";
            }
            else
            {
                result.Code = 500;
                result.Message = "客服主管已读不可撤回。";
            }
            return result;

        }

        /// <summary>
        /// 发票号全库唯一
        /// </summary>
        /// <param name="InvoiceNumber"></param>
        /// <returns></returns>
        public async Task<bool> IsSole(List<string> InvoiceNumber)
        {
            var rta = await UnitWork.Find<ReimburseFare>(r => InvoiceNumber.Contains(r.InvoiceNumber)).CountAsync();
            if (rta > 0)
            {
                return false;
            }

            var ras = await UnitWork.Find<ReimburseAccommodationSubsidy>(r => InvoiceNumber.Contains(r.InvoiceNumber)).CountAsync();
            if (ras > 0)
            {
                return false;
            }
            var roc = await UnitWork.Find<ReimburseOtherCharges>(r => InvoiceNumber.Contains(r.InvoiceNumber)).CountAsync();
            if (roc > 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 删除报销单 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Delete(ReimburseRevocationReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            var loginUser = loginContext.User;
            string UserId = "";
            StringBuilder Remark = new StringBuilder();

            #region 判断
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(req.AppId));
            }
            if (!loginContext.Roles.Any(r => r.Name.Equals("客服主管")))
            {
                UserId = loginContext.User.Id;
            }
            #endregion

            var Reimburse = await UnitWork.Find<ReimburseInfo>(r => r.Id == req.ReimburseInfoId && r.RemburseStatus == 3)
                        //.Include(r => r.ReimburseAttachments)
                        .Include(r => r.ReimburseTravellingAllowances)
                        .Include(r => r.ReimburseFares)
                        .Include(r => r.ReimburseAccommodationSubsidies)
                        .Include(r => r.ReimburseOtherCharges)
                        .Include(r => r.ReimurseOperationHistories)
                        .WhereIf(!string.IsNullOrWhiteSpace(UserId), r => r.CreateUserId.Equals(UserId))
                        .FirstOrDefaultAsync();
            if (Reimburse != null && Reimburse.MainId == 0)
            {
                var files = await UnitWork.Find<ReimburseAttachment>(null).ToListAsync();
                var delfiles = files.Where(f => f.ReimburseId.Equals(Reimburse.Id) && f.ReimburseType == 0).ToList();
                await UnitWork.BatchDeleteAsync<ReimburseAttachment>(delfiles.ToArray());
                foreach (var item in Reimburse.ReimburseFares)
                {
                    delfiles.AddRange(files.Where(f => f.ReimburseId.Equals(item.Id) && f.ReimburseType == 2).ToList());
                    Remark.Append(item.InvoiceNumber + ",");
                    await UnitWork.DeleteAsync<ReimburseFare>(item);
                }
                foreach (var item in Reimburse.ReimburseAccommodationSubsidies)
                {
                    delfiles.AddRange(files.Where(f => f.ReimburseId.Equals(item.Id) && f.ReimburseType == 3).ToList());
                    Remark.Append(item.InvoiceNumber + ",");
                    await UnitWork.DeleteAsync<ReimburseAccommodationSubsidy>(item);
                }
                foreach (var item in Reimburse.ReimburseOtherCharges)
                {
                    delfiles.AddRange(files.Where(f => f.ReimburseId.Equals(item.Id) && f.ReimburseType == 4).ToList());
                    Remark.Append(item.InvoiceNumber + ",");
                    await UnitWork.DeleteAsync<ReimburseOtherCharges>(item);
                }
                await UnitWork.BatchDeleteAsync<ReimburseAttachment>(delfiles.ToArray());
                await UnitWork.BatchDeleteAsync<ReimburseTravellingAllowance>(Reimburse.ReimburseTravellingAllowances.ToArray());
                await UnitWork.BatchDeleteAsync<ReimurseOperationHistory>(Reimburse.ReimurseOperationHistories.ToArray());
                await UnitWork.DeleteAsync<ReimburseInfo>(Reimburse);
                var CompletionReports = await UnitWork.Find<CompletionReport>(c => c.ServiceOrderId == Reimburse.ServiceOrderId && c.CreateUserId == Reimburse.CreateUserId).ToListAsync();
                CompletionReports.ForEach(c => c.IsReimburse = 1);
                await UnitWork.BatchUpdateAsync<CompletionReport>(CompletionReports.ToArray());
                Remark.Append("删除服务单为" + Reimburse.ServiceOrderSapId + "的报销单,发票号:" + Remark);
                await UnitWork.AddAsync<ReimurseOperationHistory>(new ReimurseOperationHistory
                {
                    Action = "删除报销单",
                    CreateUser = loginUser.Name,
                    CreateUserId = loginUser.Id,
                    CreateTime = DateTime.Now,
                    Remark = Remark.ToString()
                });
                await UnitWork.SaveAsync();
            }
            else
            {
                throw new CommonException("只能删除未提交和未生成报销单号的报销单！", Define.INVALID_InvoiceNumber);
            }
        }

        /// <summary>
        /// 打印报销单 
        /// </summary>
        /// <param name="ReimburseInfoId"></param>
        /// <returns></returns>
        public async Task<byte[]> Print(string ReimburseInfoId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var Reimburse = await UnitWork.Find<ReimburseInfo>(r => r.Id == int.Parse(ReimburseInfoId))
                        .Include(r => r.ReimburseTravellingAllowances)
                        .Include(r => r.ReimburseFares)
                        .Include(r => r.ReimburseAccommodationSubsidies)
                        .Include(r => r.ReimburseOtherCharges)
                        .Include(r => r.ReimurseOperationHistories)
                        .FirstOrDefaultAsync();

            var user = await UnitWork.Find<User>(u => u.Id.Equals(Reimburse.CreateUserId)).FirstOrDefaultAsync();
            var serviceorderobj = await UnitWork.Find<ServiceOrder>(u => u.Id.Equals(Reimburse.ServiceOrderId)).FirstOrDefaultAsync();
            var CompletionReports = await UnitWork.Find<CompletionReport>(c => c.ServiceOrderId == Reimburse.ServiceOrderId && c.CreateUserId.Equals(Reimburse.CreateUserId) && c.ServiceMode == 1).OrderByDescending(c => c.CreateTime).ToListAsync();
            var orgids = await UnitWork.Find<Relevance>(r => r.Key == Define.USERORG && r.FirstId == Reimburse.CreateUserId).Select(r => r.SecondId).ToListAsync();
            var orgname = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(o => orgids.Contains(o.Id)).OrderByDescending(o => o.CascadeId).Select(o => o.Name).FirstOrDefaultAsync();
            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_OtherExpenses") || u.TypeId.Equals("SYS_Transportation")).Select(u => new { u.Name, u.TypeId, u.DtValue, u.Description }).ToListAsync();

            List<ReimburseCost> ReimburseCostList = new List<ReimburseCost>();

            if (Reimburse.ReimburseFares != null && Reimburse.ReimburseFares.Count > 0)
            {
                Reimburse.ReimburseFares.ForEach(r =>
                {
                    var InvoiceTime = "";
                    if (r.InvoiceTime != null && Convert.ToDateTime(r.InvoiceTime).Hour <= 0 && Convert.ToDateTime(r.InvoiceTime).Minute <= 0 && Convert.ToDateTime(r.InvoiceTime).Second <= 0)
                    {
                        InvoiceTime = Convert.ToDateTime(r.InvoiceTime).ToString("yyyy.MM.dd");
                    }
                    else if (r.InvoiceTime == null)
                    {
                        InvoiceTime = "";
                    }
                    else
                    {
                        InvoiceTime = Convert.ToDateTime(r.InvoiceTime).ToString("yyyy.MM.dd HH:mm");
                    }
                    ReimburseCostList.Add(new ReimburseCost
                    {
                        SerialNumber = 1,
                        InvoiceTime = InvoiceTime,
                        ExpendDetails = r.From + "-" + r.To,
                        ExpendName = CategoryList.Where(u => u.TypeId.Equals("SYS_Transportation") && u.DtValue.Equals(r.Transport)).FirstOrDefault()?.Name,
                        Money = (decimal)r.Money
                    });
                });
            }
            if (Reimburse.ReimburseOtherCharges != null && Reimburse.ReimburseOtherCharges.Count > 0)
            {
                Reimburse.ReimburseOtherCharges.ForEach(r =>
                {
                    var InvoiceTime = "";
                    if (r.InvoiceTime != null && Convert.ToDateTime(r.InvoiceTime).Hour <= 0 && Convert.ToDateTime(r.InvoiceTime).Minute <= 0 && Convert.ToDateTime(r.InvoiceTime).Second <= 0)
                    {
                        InvoiceTime = Convert.ToDateTime(r.InvoiceTime).ToString("yyyy.MM.dd");
                    }
                    else if (r.InvoiceTime == null)
                    {
                        InvoiceTime = "";
                    }
                    else
                    {
                        InvoiceTime = Convert.ToDateTime(r.InvoiceTime).ToString("yyyy.MM.dd HH:mm");
                    }
                    ReimburseCostList.Add(new ReimburseCost
                    {
                        SerialNumber = 4,
                        InvoiceTime = InvoiceTime,
                        ExpendDetails = r.Remark,
                        ExpendName = r.ExpenseCategory = CategoryList.Where(u => u.TypeId.Equals("SYS_OtherExpenses") && u.DtValue.Equals(r.ExpenseCategory)).FirstOrDefault()?.Name,
                        Money = (decimal)r.Money
                    });

                });
            }
            if (Reimburse.ReimburseAccommodationSubsidies != null && Reimburse.ReimburseAccommodationSubsidies.Count > 0)
            {
                Reimburse.ReimburseAccommodationSubsidies.ForEach(r =>
                {
                    var InvoiceTime = "";
                    if (r.InvoiceTime != null && Convert.ToDateTime(r.InvoiceTime).Hour <= 0 && Convert.ToDateTime(r.InvoiceTime).Minute <= 0 && Convert.ToDateTime(r.InvoiceTime).Second <= 0)
                    {
                        InvoiceTime = Convert.ToDateTime(r.InvoiceTime).ToString("yyyy.MM.dd");
                    }
                    else if (r.InvoiceTime == null)
                    {
                        InvoiceTime = "";
                    }
                    else
                    {
                        InvoiceTime = Convert.ToDateTime(r.InvoiceTime).ToString("yyyy.MM.dd HH:mm");
                    }
                    ReimburseCostList.Add(new ReimburseCost
                    {
                        SerialNumber = 2,
                        InvoiceTime = InvoiceTime,
                        ExpendDetails = r.Money + "/天*" + r.Days + "天",
                        ExpendName = "住宿补贴",
                        Money = (decimal)r.TotalMoney
                    });
                });
            }
            var BusinessTripDate = Convert.ToDateTime(CompletionReports.Where(c => c.ServiceOrderId.Equals(Reimburse.ServiceOrderId)).Min(c => c.BusinessTripDate));
            var EndDate = Convert.ToDateTime(CompletionReports.Where(c => c.ServiceOrderId.Equals(Reimburse.ServiceOrderId)).Max(c => c.EndDate));
            if (Reimburse.ReimburseTravellingAllowances != null && Reimburse.ReimburseTravellingAllowances.Count > 0)
            {
                Reimburse.ReimburseTravellingAllowances.ForEach(r =>
                    ReimburseCostList.Add(new ReimburseCost
                    {
                        SerialNumber = 3,
                        InvoiceTime = Convert.ToDateTime(r.CreateTime).ToString("yyyy.MM.dd"),
                        ExpendDetails = r?.Money + "/天*" + r?.Days + "天",
                        ExpendName = "出差补贴",
                        Money = Convert.ToDecimal(r?.Days * r?.Money)
                    })
                );
            }
            var logopath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "logo.png");
            var logostr = "";
            using (var fs = new FileStream(logopath, FileMode.Open))
            {
                var photo = new byte[fs.Length];
                fs.Position = 0;
                await fs.ReadAsync(photo, 0, photo.Length);
                logostr = Convert.ToBase64String(photo);
                Console.WriteLine(logostr);
            }
            var FromThemeJson = JsonHelper.Instance.Deserialize<List<FromThemeJsonResp>>(CompletionReports.FirstOrDefault()?.FromTheme);
            var ReimburseCosts = ReimburseCostList.Where(r => !string.IsNullOrWhiteSpace(r.InvoiceTime.ToString())).OrderBy(r => r.InvoiceTime).ToList();
            ReimburseCosts.AddRange(ReimburseCostList.Where(r => string.IsNullOrWhiteSpace(r.InvoiceTime.ToString())).OrderBy(r => r.SerialNumber).ToList());
            var PrintReimburse = new PrintReimburseResp
            {
                ReimburseId = Reimburse.MainId,
                CompleteAddress = serviceorderobj.Province + serviceorderobj.City + serviceorderobj.Area + serviceorderobj.Addr,
                UserName = orgname + " " + user.Name,
                TerminalCustomerId = CompletionReports.FirstOrDefault()?.TerminalCustomerId,
                TerminalCustomer = CompletionReports.FirstOrDefault()?.TerminalCustomer,
                FromTheme = FromThemeJson.Take(2).Select(f => f.description).ToList(),
                logo = logostr,
                QRcode = QRCoderHelper.CreateQRCodeToBase64(Reimburse.MainId.ToString()),
                Reimburse = Reimburse,
                ReimburseCosts = ReimburseCosts
            };
            if (string.IsNullOrWhiteSpace(PrintReimburse.CompleteAddress))
            {

                var query = from a in UnitWork.Find<OCRD>(c => c.CardCode.Equals(PrintReimburse.TerminalCustomerId))
                            join f in UnitWork.Find<OCRY>(null) on a.Country equals f.Code into af
                            from f in af.DefaultIfEmpty()
                            join g in UnitWork.Find<OCST>(null) on a.State1 equals g.Code into ag
                            from g in ag.DefaultIfEmpty()
                            select new { ocryName = f.Name, ocstName = g.Name, a.City, a.Building };
                var ocrdObj = await query.FirstOrDefaultAsync();
                PrintReimburse.CompleteAddress = ocrdObj.ocryName + ocrdObj.ocstName + ocrdObj.City + ocrdObj.Building;
            }
            return await ExportAllHandler.Exporterpdf(PrintReimburse, "PrintReimburse.cshtml");
        }

        /// <summary>
        /// 导出待支付报销单 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<byte[]> Export(QueryReimburseInfoListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            #region 查询条件
            List<string> UserIds = new List<string>();
            List<int> ServiceOrderIds = new List<int>();
            List<string> OrgUserIds = new List<string>();
            var users = await UnitWork.Find<User>(null).ToListAsync();
            if (!string.IsNullOrWhiteSpace(request.CreateUserName))
            {
                UserIds.AddRange(users.Where(u => u.Name.Contains(request.CreateUserName)).Select(u => u.Id).ToList());
            }
            if (!string.IsNullOrWhiteSpace(request.TerminalCustomer))
            {
                ServiceOrderIds.AddRange(await UnitWork.Find<ServiceOrder>(s => s.TerminalCustomer.Contains(request.TerminalCustomer) || s.TerminalCustomerId.Contains(request.TerminalCustomer)).Select(s => s.Id).ToListAsync());
            }

            if (!string.IsNullOrWhiteSpace(request.OrgName))
            {
                var orgids = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(o => o.Name.Contains(request.OrgName)).Select(o => o.Id).ToListAsync();
                OrgUserIds.AddRange(await UnitWork.Find<Relevance>(r => orgids.Contains(r.SecondId) && r.Key == Define.USERORG).Select(r => r.FirstId).ToListAsync());
            }
            var result = new TableData();
            var objs = UnitWork.Find<ReimburseInfo>(null).Include(r => r.ReimburseTravellingAllowances);
            var ReimburseInfos = await objs.WhereIf(!string.IsNullOrWhiteSpace(request.MainId), r => r.MainId.ToString().Contains(request.MainId))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.ServiceOrderId), r => r.ServiceOrderSapId.ToString().Contains(request.ServiceOrderId))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.BearToPay), r => r.BearToPay.Contains(request.BearToPay))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.Responsibility), r => r.Responsibility.Contains(request.Responsibility))
                      .WhereIf(request.StartDate != null, r => r.CreateTime > request.StartDate)
                      .WhereIf(request.EndDate != null, r => r.CreateTime < Convert.ToDateTime(request.EndDate).AddMinutes(1440))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.ReimburseType), r => r.ReimburseType.Equals(request.ReimburseType))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.CreateUserName), r => UserIds.Contains(r.CreateUserId))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.OrgName), r => OrgUserIds.Contains(r.CreateUserId))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.TerminalCustomer), r => ServiceOrderIds.Contains(r.ServiceOrderId))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.ServiceRelations), r => r.ServiceRelations.Contains(request.ServiceRelations))
                      .Where(r => r.RemburseStatus == 8).ToListAsync();
            #endregion

            var query = from a in ReimburseInfos
                        join b in users on a.CreateUserId equals b.Id into ab
                        from b in ab.DefaultIfEmpty()
                        select new { a.CreateUserId, b.Name, a.TotalMoney, b.CardNo };
            var Totalquery = query.GroupBy(q => new { q.CreateUserId, q.Name, q.CardNo }).Select(q => new { q.Key.CardNo, q.Key.Name, TotalMoney = q.Select(s => s.TotalMoney).Sum().ToString("F2") });
            return await NPOIHelper.ExporterExcel(Totalquery.ToList());
        }

        /// <summary>
        /// 导出我的提交报销单(暂用)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<byte[]> ExportLoad(QueryReimburseInfoListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            #region 查询条件
            List<string> UserIds = new List<string>();
            List<int> ServiceOrderIds = new List<int>();
            List<string> OrgUserIds = new List<string>();
            if (!string.IsNullOrWhiteSpace(request.CreateUserName))
            {
                UserIds.AddRange(await UnitWork.Find<User>(u => u.Name.Contains(request.CreateUserName)).Select(u => u.Id).ToListAsync());
            }

            if (!string.IsNullOrWhiteSpace(request.TerminalCustomer))
            {
                ServiceOrderIds.AddRange(await UnitWork.Find<ServiceOrder>(s => s.TerminalCustomer.Contains(request.TerminalCustomer) || s.TerminalCustomerId.Contains(request.TerminalCustomer)).Select(s => s.Id).ToListAsync());
            }

            if (!string.IsNullOrWhiteSpace(request.OrgName))
            {
                var orgids = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(o => o.Name.Contains(request.OrgName)).Select(o => o.Id).ToListAsync();
                OrgUserIds.AddRange(await UnitWork.Find<Relevance>(r => orgids.Contains(r.SecondId) && r.Key == Define.USERORG).Select(r => r.FirstId).ToListAsync());
            }
            var objs = UnitWork.Find<ReimburseInfo>(null).Include(r => r.ReimburseTravellingAllowances);
            var ReimburseInfos = objs.WhereIf(!string.IsNullOrWhiteSpace(request.MainId), r => r.MainId.ToString().Contains(request.MainId))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.ServiceOrderId), r => r.ServiceOrderSapId.ToString().Contains(request.ServiceOrderId))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.BearToPay), r => r.BearToPay.Contains(request.BearToPay))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.Responsibility), r => r.Responsibility.Contains(request.Responsibility))
                      .WhereIf(request.StartDate != null, r => r.CreateTime > request.StartDate)
                      .WhereIf(request.EndDate != null, r => r.CreateTime < Convert.ToDateTime(request.EndDate).AddMinutes(1440))
                      //.WhereIf(!string.IsNullOrWhiteSpace(request.IsDraft.ToString()), r => r.IsDraft == request.IsDraft)
                      .WhereIf(!string.IsNullOrWhiteSpace(request.ReimburseType), r => r.ReimburseType.Equals(request.ReimburseType))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.CreateUserName), r => UserIds.Contains(r.CreateUserId))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.OrgName), r => OrgUserIds.Contains(r.CreateUserId))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.TerminalCustomer), r => ServiceOrderIds.Contains(r.ServiceOrderId))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.ServiceRelations), r => r.ServiceRelations.Contains(request.ServiceRelations))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.Status), r => r.RemburseStatus.Equals(request.Status))
                      .WhereIf(request.PaymentStartDate != null, r => r.PayTime > request.PaymentStartDate)
                      .WhereIf(request.PaymentEndDate != null, r => r.PayTime < Convert.ToDateTime(request.PaymentEndDate).AddDays(1));

            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ServiceRelations") && u.Enable == false).Select(u => u.Name).ToListAsync();
            var categoryStatus = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_RemburseStatus") && u.Enable == true).Select(u => new { u.Name, u.DtValue }).ToListAsync();

            if (CategoryList != null && CategoryList.Where(c => c.Equals("All")).Count() >= 1)
            {
                ReimburseInfos = ReimburseInfos.Where(r => CategoryList.Contains(r.ServiceRelations));
            }
            else
            {
                ReimburseInfos = ReimburseInfos.Where(r => r.ServiceRelations.Equals(loginContext.User.ServiceRelations));
            }

            if (!string.IsNullOrWhiteSpace(request.RemburseStatus))
            {
                switch (request.RemburseStatus)
                {
                    case "1":
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 1 || r.RemburseStatus == 2);
                        break;
                    case "3":
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 3);
                        break;
                    case "4":
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus >= 4 && r.RemburseStatus < 9);
                        break;
                    case "9":
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 9);
                        break;
                }
            }
            if (request.PageType == 1 && !loginContext.Roles.Any(r => r.Name.Equals("客服主管")) && loginContext.User.Account != Define.SYSTEM_USERNAME)
            {
                ReimburseInfos = ReimburseInfos.Where(r => r.CreateUserId.Equals(loginContext.User.Id));
            };
            #endregion

            var ReimburseInfoList = await ReimburseInfos.OrderByDescending(r => r.UpdateTime).ToListAsync();
            ServiceOrderIds = ReimburseInfos.Select(d => d.ServiceOrderId).ToList();
            var SelOrgName = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(null).Select(o => new { o.Id, o.Name, o.CascadeId }).ToListAsync();
            var SelUserName = await UnitWork.Find<User>(null).Select(u => new { u.Id, u.Name }).ToListAsync();
            var Relevances = await UnitWork.Find<Relevance>(r => r.Key == Define.USERORG).Select(r => new { r.FirstId, r.SecondId }).ToListAsync();
            var CompletionReports = await UnitWork.Find<CompletionReport>(c => ServiceOrderIds.Contains((int)c.ServiceOrderId) && c.ServiceMode == 1).ToListAsync();
            var ServiceOrders = await UnitWork.Find<ServiceOrder>(s => ServiceOrderIds.Contains(s.Id)).ToListAsync();
            var ReimburseResps = from a in ReimburseInfoList
                                 join b in CompletionReports on a.ServiceOrderId equals b.ServiceOrderId into ab
                                 from b in ab.DefaultIfEmpty()
                                 join c in ServiceOrders on a.ServiceOrderId equals c.Id into ac
                                 from c in ac.DefaultIfEmpty()
                                 join d in SelUserName on a.CreateUserId equals d.Id into ad
                                 from d in ad.DefaultIfEmpty()
                                 join e in Relevances on a.CreateUserId equals e.FirstId into ae
                                 from e in ae.DefaultIfEmpty()
                                 join f in SelOrgName on e.SecondId equals f.Id into ef
                                 from f in ef.DefaultIfEmpty()
                                 select new { a, b, c, d, f };

            ReimburseResps = ReimburseResps.OrderByDescending(r => r.f.CascadeId).ToList();
            ReimburseResps = ReimburseResps.GroupBy(r => r.a.Id).Select(r => r.First()).OrderByDescending(r => r.a.UpdateTime).ToList();

            var ReimburseRespList = ReimburseResps.Select(r => new
            {
                报销单号=r.a.MainId,
                服务ID=r.a.ServiceOrderSapId,
                报销状态= categoryStatus.Where(c=>c.DtValue==r.a.RemburseStatus.ToString()).FirstOrDefault()?.Name,
                创建时间 = r.a.CreateTime.ToString("yyyy-MM-dd"),
                客户代码 = r.b.TerminalCustomerId,
                客户名称 = r.b.TerminalCustomer,
                出差开始时间 = Convert.ToDateTime(CompletionReports.Where(c => c.ServiceOrderId.Equals(r.a.ServiceOrderId)).Min(c => c.BusinessTripDate)).ToString("yyyy-MM-dd"),
                出差结束时间 = Convert.ToDateTime(CompletionReports.Where(c => c.ServiceOrderId.Equals(r.a.ServiceOrderId)).Max(c => c.EndDate)).ToString("yyyy-MM-dd"),
                出差天数 = r.a.ReimburseTravellingAllowances.FirstOrDefault()?.Days,
                呼叫主题 = JsonHelper.Instance.Deserialize<List<FromThemeJsonResp>>(r.b.FromTheme).Select(r => r.description).FirstOrDefault(),
                销售员 = r.c.SalesMan,
                报销人 = r.d.Name,
                部门 = r.f.Name,
                金额 = r.a.TotalMoney
            }).ToList();
            if (request.CompletionStartDate != null) ReimburseRespList = ReimburseRespList.Where(r => Convert.ToDateTime(r.出差开始时间) >= request.CompletionStartDate).ToList();
            if (request.CompletionEndDate != null) ReimburseRespList = ReimburseRespList.Where(r => Convert.ToDateTime(r.出差结束时间) < Convert.ToDateTime(request.CompletionEndDate).AddDays(1)).ToList();
            var bytes = await ExportAllHandler.ExporterExcel(ReimburseRespList);
            return bytes;
        }

        /// <summary>
        /// 获取用户
        /// </summary>
        /// <param name="AppId"></param>
        /// <returns></returns>
        private async Task<User> GetUserId(int AppId)
        {
            var userid = UnitWork.Find<AppUserMap>(u => u.AppUserId.Equals(AppId)).Select(u => u.UserID).FirstOrDefault();

            return await UnitWork.Find<User>(u => u.Id.Equals(userid)).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 判断劳务关系是否正确
        /// </summary>
        /// <param name="AppId"></param>
        /// <param name="ServiceRelations"></param>
        /// <param name="CompanyTaxCode"></param>
        /// <returns></returns>
        public async Task<IsServiceRelationsResp> IsServiceRelations(string AppId, string ServiceRelations, string CompanyTaxCode)
        {
            var user = _auth.GetCurrentUser().User;
            if (user.Account == Define.USERAPP)
            {
                user = await GetUserId(Convert.ToInt32(AppId));
            }
            var categoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ServiceRelations"))
                .Select(u => new { u.Name, u.DtCode, u.Description }).ToListAsync();
            var Relations = categoryList.Where(u => u.Name.Equals(user.ServiceRelations)).FirstOrDefault().Description;
            if (!ServiceRelations.Equals(Relations))
            {
                return new IsServiceRelationsResp { ispass = false, message = "发票抬头与本人劳务关系不一致，禁止报销" };
            }
            var companyTaxCodes = categoryList.Select(c => c.DtCode).ToList();
            if (!companyTaxCodes.Contains(CompanyTaxCode))
            {
                return new IsServiceRelationsResp { ispass = false, message = "发票抬头和系统维护的不一样，禁止报销" };
            }
            return new IsServiceRelationsResp { ispass = true };
        }

        /// <summary>
        /// 添加修改通用条件
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private ReimburseInfo Condition(AddOrUpdateReimburseInfoReq req)
        {

            #region 过滤掉不需要添加的数据
            req.ReimburseFares = req.ReimburseFares.Where(r => r.IsAdd == null || r.IsAdd == true).ToList();
            req.ReimburseAccommodationSubsidies = req.ReimburseAccommodationSubsidies.Where(r => r.IsAdd == null || r.IsAdd == true).ToList();
            req.ReimburseOtherCharges = req.ReimburseOtherCharges.Where(r => r.IsAdd == null || r.IsAdd == true).ToList();
            #endregion

            #region 删除我的费用(假删除)
            if (req.MyexpendsIds != null && req.MyexpendsIds.Count > 0)
            {
                var myexpends = UnitWork.Find<MyExpends>(m => req.MyexpendsIds.Contains(m.Id)).ToList();
                myexpends.ForEach(m => m.IsDelete = true);
                UnitWork.BatchUpdate(myexpends.ToArray());
            }
            #endregion

            #region 判断发票是否唯一
            List<string> InvoiceNumbers = new List<string>();
            InvoiceNumbers.AddRange(req.ReimburseFares.Where(r => string.IsNullOrWhiteSpace(r.Id.ToString()) || r.Id.ToString() == "0").Select(r => r.InvoiceNumber).ToList());
            InvoiceNumbers.AddRange(req.ReimburseAccommodationSubsidies.Where(r => string.IsNullOrWhiteSpace(r.Id.ToString()) || r.Id.ToString() == "0").Select(r => r.InvoiceNumber).ToList());
            InvoiceNumbers.AddRange(req.ReimburseOtherCharges.Where(r => string.IsNullOrWhiteSpace(r.Id.ToString()) || r.Id.ToString() == "0").Select(r => r.InvoiceNumber).ToList());
            bool IsInvoiceNumber = InvoiceNumbers.GroupBy(i => i).Where(g => g.Count() > 1).Count() >= 1;
            if (IsInvoiceNumber)
            {
                string msg = "";
                InvoiceNumbers.GroupBy(i => i).Where(g => g.Count() > 1).Select(i => i.Key).ToList().ForEach(i => msg += i + ",");
                throw new CommonException($"添加报销单失败。发票号：{msg}重复！", Define.INVALID_InvoiceNumber);
            }
            else if (InvoiceNumbers.Count() > 0)
            {
                if (!IsSole(InvoiceNumbers).ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    throw new CommonException("添加报销单失败。发票已使用，不可二次使用！", Define.INVALID_InvoiceNumber);
                }
            }
            #endregion

            #region 必须存在附件并排序
            int racount = 0;
            int SerialNumber = 0;
            req.ReimburseOtherCharges.ForEach(r => { racount += r.ReimburseAttachments.Count() <= 0 ? 1 : 0; r.SerialNumber = ++SerialNumber; });
            SerialNumber = 0;
            req.ReimburseFares.ForEach(r => { racount += r.ReimburseAttachments.Count() <= 0 ? 1 : 0; r.SerialNumber = ++SerialNumber; });
            SerialNumber = 0;
            req.ReimburseAccommodationSubsidies.ForEach(r => { racount += r.ReimburseAttachments.Count() <= 0 ? 1 : 0; r.SerialNumber = ++SerialNumber; });
            if (racount > 0)
            {
                throw new CommonException("请上传附件！", Define.INVALID_ReimburseAgain);
            }
            #endregion

            #region 判断金额
            if (req.TotalMoney <= 0)
            {
                throw new Exception("金额为0不可提交");
            }
            #endregion

            #region 计算金额
            decimal totalMoeny = 0;
            req.ReimburseTravellingAllowances.ForEach(r => totalMoeny += (decimal)(r.Days * r.Money));
            req.ReimburseOtherCharges.ForEach(r => totalMoeny += (decimal)r.Money);
            req.ReimburseFares.ForEach(r => totalMoeny += (decimal)r.Money);
            req.ReimburseAccommodationSubsidies.ForEach(r => totalMoeny += (decimal)r.TotalMoney);
            req.TotalMoney = totalMoeny;
            #endregion
            return req.MapTo<ReimburseInfo>();
        }

        /// <summary>
        /// 客户历史报销单 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> HistoryReimburseInfo(QueryReimburseInfoListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var CompletionReports = await UnitWork.Find<CompletionReport>(c => c.TerminalCustomerId.Equals(req.TerminalCustomer) && (c.IsReimburse == 2 || c.IsReimburse == 4)).Select(c => new
            {
                c.ServiceOrderId,
                c.CreateUserId,
                c.EndDate,
                c.TechnicianName,
                c.BusinessTripDate,
                c.IsReimburse,
                c.BusinessTripDays
            }).ToListAsync();
            var ServiceOrderIds = CompletionReports.Where(c => c.IsReimburse == 2).Select(c => c.ServiceOrderId).Distinct().ToList();
            var ReimburseInfos = await UnitWork.Find<ReimburseInfo>(r => ServiceOrderIds.Contains(r.ServiceOrderId) && r.RemburseStatus > 8)
                               .Include(r => r.ReimburseTravellingAllowances)
                               .Include(r => r.ReimburseFares)
                               .Include(r => r.ReimburseAccommodationSubsidies)
                               .Include(r => r.ReimburseOtherCharges)
                               .OrderByDescending(r => r.MainId)
                               //.Skip((req.page - 1) * req.limit)
                               //.Take(req.limit)
                               .ToListAsync();

            //List<string> site = new List<string>();
            //foreach (var item in ReimburseInfos)
            //{
            //    item.ReimburseFares.ForEach(r=>site.Add(r.From+r.To));
            //}
            //var ReimburseFaresList = await UnitWork.Find<ReimburseFare>(r=> site.Contains(r.From+r.To)).ToListAsync();

            //var meanVale = ReimburseFaresList.GroupBy(r => new { r.From, r.To }).Select(r => new { r.Key.From, r.Key.To, Count = (r.Select(r => r.Money).Sum() / r.Select(r => r.Money).Count()) });
            var serviceIds = ReimburseInfos.Select(r => r.ServiceOrderId).ToList();
            var serviceDailyExpends = await UnitWork.Find<ServiceDailyExpends>(s => serviceIds.Contains(s.ServiceOrderId) && s.DailyExpenseType == 1).ToListAsync();
            var userId = ReimburseInfos.Select(r => r.CreateUserId).ToList();
            var query = from a in UnitWork.Find<Relevance>(r => r.Key == Define.USERORG && userId.Contains(r.FirstId))
                        join b in UnitWork.Find<OpenAuth.Repository.Domain.Org>(null) on a.SecondId equals b.Id into ab
                        from b in ab.DefaultIfEmpty()
                        select new { a, b };

            var ReimburseInfoList = ReimburseInfos.Select(r => new
            {
                r.MainId,
                Days = r.ReimburseTravellingAllowances.Sum(t => t.Days) <= 0 && serviceDailyExpends.Where(s => s.ServiceOrderId == r.ServiceOrderId) != null ? serviceDailyExpends.Where(s => s.ServiceOrderId == r.ServiceOrderId).Sum(s => s.Days) : r.ReimburseTravellingAllowances.Sum(t => t.Days),
                r.TotalMoney,
                FaresMoney = r.ReimburseFares.Sum(f => f.Money),
                TravellingAllowancesMoney = r.ReimburseTravellingAllowances.FirstOrDefault()?.Days.Value * r.ReimburseTravellingAllowances.FirstOrDefault()?.Money.Value,
                AccommodationSubsidiesMoney = r.ReimburseAccommodationSubsidies.Sum(a => a.TotalMoney),
                OtherChargesMoney = r.ReimburseOtherCharges.Sum(o => o.Money),
                BusinessTripDate = CompletionReports.Where(c => c.CreateUserId.Equals(r.CreateUserId) && c.ServiceOrderId.Equals(r.ServiceOrderId)).Min(c => c.BusinessTripDate),
                EndDate = CompletionReports.Where(c => c.CreateUserId.Equals(r.CreateUserId) && c.ServiceOrderId.Equals(r.ServiceOrderId)).Max(c => c.EndDate),
                UserName = CompletionReports.Where(c => c.CreateUserId.Equals(r.CreateUserId) && c.ServiceOrderId.Equals(r.ServiceOrderId)).FirstOrDefault()?.TechnicianName,
                OrgName = query.Where(q => q.a.FirstId == r.CreateUserId).FirstOrDefault()?.b?.Name
            }).ToList();


            var ReimburseInfoRes = ReimburseInfoList.Select(r => new
            {
                r.MainId,
                Type = "报销",
                r.Days,
                r.TotalMoney,
                r.FaresMoney,
                AverageDaily = r.Days > 0 ? r.TotalMoney / r.Days : r.TotalMoney,
                FMProportion = Convert.ToDecimal((r.FaresMoney / r.TotalMoney)).ToString("p"),
                r.TravellingAllowancesMoney,
                TAProportion = Convert.ToDecimal((r.TravellingAllowancesMoney / r.TotalMoney)).ToString("p"),
                r.AccommodationSubsidiesMoney,
                ASProportion = Convert.ToDecimal((r.AccommodationSubsidiesMoney / r.TotalMoney)).ToString("p"),
                r.OtherChargesMoney,
                OCProportion = Convert.ToDecimal((r.OtherChargesMoney / r.TotalMoney)).ToString("p"),
                r.BusinessTripDate,
                r.EndDate,
                r.UserName,
                r.OrgName
            }).OrderByDescending(r => r.MainId).ToList();

            //个代结算
            ServiceOrderIds= CompletionReports.Where(c => c.IsReimburse == 4).Select(c => c.ServiceOrderId).Distinct().ToList();
            var outsourceId = await UnitWork.Find<OutsourcExpenses>(c => ServiceOrderIds.Contains(c.ServiceOrderId)).Select(c => c.OutsourcId).ToListAsync();
            var flowInstaceId = await UnitWork.Find<FlowInstance>(c => c.CustomName == "个人代理结算单" && c.ActivityName == "结束").Select(c => c.Id).ToListAsync();
            var outsource = await UnitWork.Find<Outsourc>(c => flowInstaceId.Contains(c.FlowInstanceId) && outsourceId.Contains(c.Id)).Include(c => c.OutsourcExpenses).ToListAsync();
            var outsourceList = outsource.Select(r =>
            {
                var serviceOrderId = r.OutsourcExpenses.FirstOrDefault()?.ServiceOrderId;
                var completionReports = CompletionReports.Where(c => c.CreateUserId.Equals(r.CreateUserId) && c.ServiceOrderId.Equals(serviceOrderId)).FirstOrDefault();
                return new
                {
                    MainId = r.Id,
                    r.TotalMoney,
                    Days = completionReports?.BusinessTripDays,
                    FaresMoney = r.OutsourcExpenses.Where(o => o.ExpenseType == 1).Sum(o => o.Money),//交通
                    AccommodationSubsidiesMoney = r.OutsourcExpenses.Where(o => o.ExpenseType == 2).Sum(o => o.Money),//住宿
                    TravellingAllowancesMoney= r.OutsourcExpenses.Where(o => o.ExpenseType == 0).Sum(o => o.Money),//出差补贴为金额0
                    OtherChargesMoney = r.OutsourcExpenses.Where(o => o.ExpenseType == 3 || o.ExpenseType == 4).Sum(o => o.Money),//其他
                    BusinessTripDate = CompletionReports.Where(c => c.CreateUserId.Equals(r.CreateUserId) && c.ServiceOrderId.Equals(serviceOrderId)).Min(c => c.BusinessTripDate),
                    EndDate = CompletionReports.Where(c => c.CreateUserId.Equals(r.CreateUserId) && c.ServiceOrderId.Equals(serviceOrderId)).Max(c => c.EndDate),
                    UserName = completionReports?.TechnicianName,
                    OrgName = query.Where(q => q.a.FirstId == r.CreateUserId).FirstOrDefault()?.b?.Name
                };
            });

            var outsourceRes = outsourceList.Select(r => new
            {
                r.MainId,
                Type = "结算",
                r.Days,
                r.TotalMoney,
                r.FaresMoney,
                AverageDaily = r.Days > 0 ? r.TotalMoney / r.Days : r.TotalMoney,
                FMProportion = Convert.ToDecimal((r.FaresMoney / r.TotalMoney)).ToString("p"),
                r.TravellingAllowancesMoney,
                TAProportion = "0.00%",
                r.AccommodationSubsidiesMoney,
                ASProportion = Convert.ToDecimal((r.AccommodationSubsidiesMoney / r.TotalMoney)).ToString("p"),
                r.OtherChargesMoney,
                OCProportion = Convert.ToDecimal((r.OtherChargesMoney / r.TotalMoney)).ToString("p"),
                r.BusinessTripDate,
                r.EndDate,
                r.UserName,
                r.OrgName
            }).OrderByDescending(r => r.MainId).ToList();
            ReimburseInfoRes.AddRange(outsourceRes);
            var resultData= ReimburseInfoRes
                               .Skip((req.page - 1) * req.limit)
                               .Take(req.limit);
            result.Count = ReimburseInfoRes.Count;
            result.Data = new
            {
                TotalMoney = ReimburseInfoRes.Sum(c => c.TotalMoney),
                resultData
            };
            return result;

        }

        /// <summary>
        /// 个人历史费用
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> HistoryReimburseInfoForUser(QueryReimburseInfoListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();
            //报销单
            var ReimburseInfos = await UnitWork.Find<ReimburseInfo>(r => req.CreateUserId==r.CreateUserId && r.RemburseStatus == 9)
                               .Include(r => r.ReimburseTravellingAllowances)
                               .Include(r => r.ReimburseFares)
                               .Include(r => r.ReimburseAccommodationSubsidies)
                               .Include(r => r.ReimburseOtherCharges)
                               //.OrderByDescending(r => r.MainId)
                               .ToListAsync();

            //结算单
            var flowInstaceId = await UnitWork.Find<FlowInstance>(c => c.CustomName == "个人代理结算单" && c.ActivityName == "结束").Select(c => c.Id).ToListAsync();
            var outsource = await UnitWork.Find<Outsourc>(c => flowInstaceId.Contains(c.FlowInstanceId)).Include(c => c.OutsourcExpenses).ToListAsync();

            var ServiceOrderIds = ReimburseInfos.Select(c => c?.ServiceOrderId).ToList();
            var serviceDailyExpends = await UnitWork.Find<ServiceDailyExpends>(s => ServiceOrderIds.Contains(s.ServiceOrderId) && s.DailyExpenseType == 1).ToListAsync();

            outsource.ForEach(o =>
            {
                ServiceOrderIds.AddRange(o.OutsourcExpenses.Select(c => c.ServiceOrderId).ToList());
            });

            var CompletionReports = await UnitWork.Find<CompletionReport>(c => ServiceOrderIds.Contains(c.ServiceOrderId) && (c.IsReimburse == 2 || c.IsReimburse == 4)).Select(c => new
            {
                c.ServiceOrderId,
                c.CreateUserId,
                c.EndDate,
                c.TechnicianName,
                c.BusinessTripDate,
                c.IsReimburse,
                c.BusinessTripDays,
                c.TerminalCustomerId,
                c.TerminalCustomer
            }).ToListAsync();

            var petitioner = await (from a in UnitWork.Find<User>(u => u.Id.Equals(req.CreateUserId))
                                    join b in UnitWork.Find<Relevance>(r => r.Key == Define.USERORG) on a.Id equals b.FirstId into ab
                                    from b in ab.DefaultIfEmpty()
                                    join c in UnitWork.Find<OpenAuth.Repository.Domain.Org>(null) on b.SecondId equals c.Id into bc
                                    from c in bc.DefaultIfEmpty()
                                    select new { a.Name, a.Id, OrgName = c.Name, c.CascadeId }).OrderByDescending(u => u.CascadeId).FirstOrDefaultAsync();
            var orgname= petitioner.OrgName + "-" + petitioner.Name;
            var ReimburseInfoList = ReimburseInfos.Select(r => new
            {
                r.Id,
                r.MainId,
                Days = r.ReimburseTravellingAllowances.Sum(t => t.Days) <= 0 && serviceDailyExpends.Where(s => s.ServiceOrderId == r.ServiceOrderId) != null ? serviceDailyExpends.Where(s => s.ServiceOrderId == r.ServiceOrderId).Sum(s => s.Days) : r.ReimburseTravellingAllowances.Sum(t => t.Days),
                r.TotalMoney,
                FaresMoney = r.ReimburseFares.Sum(f => f.Money),
                TravellingAllowancesMoney = r.ReimburseTravellingAllowances.FirstOrDefault()?.Days.Value * r.ReimburseTravellingAllowances.FirstOrDefault()?.Money.Value,
                AccommodationSubsidiesMoney = r.ReimburseAccommodationSubsidies.Sum(a => a.TotalMoney),
                OtherChargesMoney = r.ReimburseOtherCharges.Sum(o => o.Money),
                BusinessTripDate = CompletionReports.Where(c => c.CreateUserId.Equals(r.CreateUserId) && c.ServiceOrderId.Equals(r.ServiceOrderId)).Min(c => c.BusinessTripDate),
                EndDate = CompletionReports.Where(c => c.CreateUserId.Equals(r.CreateUserId) && c.ServiceOrderId.Equals(r.ServiceOrderId)).Max(c => c.EndDate),
                //UserName = CompletionReports.Where(c => c.CreateUserId.Equals(r.CreateUserId) && c.ServiceOrderId.Equals(r.ServiceOrderId)).FirstOrDefault()?.TechnicianName,
                TerminalCustomerId = CompletionReports.Where(c => c.CreateUserId.Equals(r.CreateUserId) && c.ServiceOrderId.Equals(r.ServiceOrderId)).FirstOrDefault().TerminalCustomerId,
                TerminalCustomer = CompletionReports.Where(c => c.CreateUserId.Equals(r.CreateUserId) && c.ServiceOrderId.Equals(r.ServiceOrderId)).FirstOrDefault().TerminalCustomer,
                OrgName = orgname
            }).ToList();


            var ReimburseInfoRes = ReimburseInfoList.Select(r => new
            {
                r.Id,
                r.MainId,
                Type = "报销",
                r.Days,
                r.TotalMoney,
                r.FaresMoney,
                AverageDaily = r.Days > 0 ? r.TotalMoney / r.Days : r.TotalMoney,
                FMProportion = Convert.ToDecimal((r.FaresMoney / r.TotalMoney)).ToString("p"),
                r.TravellingAllowancesMoney,
                TAProportion = Convert.ToDecimal((r.TravellingAllowancesMoney / r.TotalMoney)).ToString("p"),
                r.AccommodationSubsidiesMoney,
                ASProportion = Convert.ToDecimal((r.AccommodationSubsidiesMoney / r.TotalMoney)).ToString("p"),
                r.OtherChargesMoney,
                OCProportion = Convert.ToDecimal((r.OtherChargesMoney / r.TotalMoney)).ToString("p"),
                r.BusinessTripDate,
                r.EndDate,
                //r.UserName,
                r.OrgName,
                r.TerminalCustomerId,
                r.TerminalCustomer
            }).OrderByDescending(r => r.MainId).ToList();

            var outsourceList = outsource.Select(r =>
            {
                var serviceOrderId = r.OutsourcExpenses.FirstOrDefault()?.ServiceOrderId;
                var completionReports = CompletionReports.Where(c => c.CreateUserId.Equals(r.CreateUserId) && c.ServiceOrderId.Equals(serviceOrderId)).FirstOrDefault();
                return new
                {
                    r.Id,
                    MainId = r.Id,
                    r.TotalMoney,
                    Days = completionReports?.BusinessTripDays,
                    FaresMoney = r.OutsourcExpenses.Where(o => o.ExpenseType == 1).Sum(o => o.Money),//交通
                    AccommodationSubsidiesMoney = r.OutsourcExpenses.Where(o => o.ExpenseType == 2).Sum(o => o.Money),//住宿
                    TravellingAllowancesMoney = r.OutsourcExpenses.Where(o => o.ExpenseType == 0).Sum(o => o.Money),//出差补贴为金额0
                    OtherChargesMoney = r.OutsourcExpenses.Where(o => o.ExpenseType == 3 || o.ExpenseType == 4).Sum(o => o.Money),//其他
                    BusinessTripDate = CompletionReports.Where(c => c.CreateUserId.Equals(r.CreateUserId) && c.ServiceOrderId.Equals(serviceOrderId)).Min(c => c.BusinessTripDate),
                    EndDate = CompletionReports.Where(c => c.CreateUserId.Equals(r.CreateUserId) && c.ServiceOrderId.Equals(serviceOrderId)).Max(c => c.EndDate),
                    //UserName = completionReports?.TechnicianName,
                    TerminalCustomerId = completionReports.TerminalCustomerId,
                    TerminalCustomer = completionReports.TerminalCustomer,
                    OrgName = orgname
                };
            });

            var outsourceRes = outsourceList.Select(r => new
            {
                r.Id,
                r.MainId,
                Type = "结算",
                r.Days,
                r.TotalMoney,
                r.FaresMoney,
                AverageDaily = r.Days > 0 ? r.TotalMoney / r.Days : r.TotalMoney,
                FMProportion = Convert.ToDecimal((r.FaresMoney / r.TotalMoney)).ToString("p"),
                r.TravellingAllowancesMoney,
                TAProportion = "0.00%",
                r.AccommodationSubsidiesMoney,
                ASProportion = Convert.ToDecimal((r.AccommodationSubsidiesMoney / r.TotalMoney)).ToString("p"),
                r.OtherChargesMoney,
                OCProportion = Convert.ToDecimal((r.OtherChargesMoney / r.TotalMoney)).ToString("p"),
                r.BusinessTripDate,
                r.EndDate,
                //r.UserName,
                r.OrgName,
                r.TerminalCustomerId,
                r.TerminalCustomer
            }).OrderByDescending(r => r.MainId).ToList();
            ReimburseInfoRes.AddRange(outsourceRes);
            var totalMoney = ReimburseInfoRes.Sum(c => c.TotalMoney);
            var detail = new
            {
                FaresMoney = ReimburseInfoRes.Sum(c => c.FaresMoney),
                FMProportion = totalMoney > 0 ? Convert.ToDecimal((ReimburseInfoRes.Sum(c => c.FaresMoney) / totalMoney)).ToString("p") : "0",
                TravellingAllowancesMoney = ReimburseInfoRes.Sum(c => c.TravellingAllowancesMoney),
                TAProportion = totalMoney > 0 ? Convert.ToDecimal((ReimburseInfoRes.Sum(c => c.TravellingAllowancesMoney) / totalMoney)).ToString("p") : "0",
                AccommodationSubsidiesMoney = ReimburseInfoRes.Sum(c => c.AccommodationSubsidiesMoney),
                ASProportion = totalMoney > 0 ? Convert.ToDecimal((ReimburseInfoRes.Sum(c => c.AccommodationSubsidiesMoney) / totalMoney)).ToString("p") : "0",
                OtherChargesMoney = ReimburseInfoRes.Sum(c => c.OtherChargesMoney),
                OCProportion = totalMoney > 0 ? Convert.ToDecimal((ReimburseInfoRes.Sum(c => c.OtherChargesMoney) / totalMoney)).ToString("p") : "0",
            };

            var resultData = ReimburseInfoRes
                               .Skip((req.page - 1) * req.limit)
                               .Take(req.limit);
            result.Count = ReimburseInfoRes.Count;
            result.Data = new
            {
                TotalMoney = totalMoney,
                Detail = detail,
                ResultData = resultData
            };
            return result;
        }

        /// <summary>
        /// 报表分析
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> AnalysisReport()
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            List<AnalysisReportResp> AnalysisReportRespList = new List<AnalysisReportResp>();
            //按客户
            var Customer = from a in UnitWork.Find<ReimburseInfo>(null)
                           join b in UnitWork.Find<CompletionReport>(c => c.IsReimburse == 2) on a.ServiceOrderId equals b.ServiceOrderId into ab
                           from b in ab.DefaultIfEmpty()
                           select new { a, b };
            var CustomerList = await Customer.ToListAsync();
            CustomerList = CustomerList.GroupBy(c => c.a.Id).Select(c => c.First()).ToList();
            var CustomerReportList = CustomerList.GroupBy(c => c.b.TerminalCustomerId).Select(c => new AnalysisReportSublist { Name = c.Key, Count = c.Select(s => s.a.Id).Count() }).ToList();
            AnalysisReportRespList.Add(new AnalysisReportResp { Name = "CustomerReport", AnalysisReportSublists = CustomerReportList });

            //按业务员
            var SalesMans = from a in UnitWork.Find<ReimburseInfo>(null)
                            join b in UnitWork.Find<ServiceOrder>(null) on a.ServiceOrderId equals b.Id into ab
                            from b in ab.DefaultIfEmpty()
                            select new { a, b };
            var SalesManList = await SalesMans.ToListAsync();
            var SalesManReportList = SalesManList.GroupBy(s => s.b.SalesMan).Select(s => new AnalysisReportSublist { Name = s.Key, Count = s.Select(u => u.a.Id).Count() }).ToList();
            AnalysisReportRespList.Add(new AnalysisReportResp { Name = "SalesManReport", AnalysisReportSublists = SalesManReportList });

            var userIds = await UnitWork.Find<ReimburseInfo>(null).Select(s => s.CreateUserId).ToListAsync();

            //按部门
            var OrgNames = from b in UnitWork.Find<Relevance>(r => userIds.Contains(r.FirstId))
                           join c in UnitWork.Find<OpenAuth.Repository.Domain.Org>(null) on b.SecondId equals c.Id into bc
                           from c in bc.DefaultIfEmpty()
                           where b.Key.Equals(Define.USERORG)
                           select new { b, c };
            var OrgNameList = await OrgNames.OrderByDescending(o => o.c.CascadeId).ToListAsync();
            OrgNameList = OrgNameList.GroupBy(o => o.b.FirstId).Select(o => o.First()).ToList();
            var OrgNameReportList = OrgNameList.GroupBy(o => new { o.c.Id, o.c.Name }).Select(o => new AnalysisReportSublist { Name = o.Key.Name, Count = o.Select(u => u.b.Id).Count() }).ToList();
            AnalysisReportRespList.Add(new AnalysisReportResp { Name = "OrgNameReport", AnalysisReportSublists = OrgNameReportList });

            result.Data = AnalysisReportRespList;
            return result;

        }

        public ReimburseInfoApp(IUnitWork unitWork, ModuleFlowSchemeApp moduleFlowSchemeApp, WorkbenchApp workbenchApp, FlowInstanceApp flowInstanceApp, IAuth auth, QuotationApp quotationApp) : base(unitWork, auth)
        {
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
            _flowInstanceApp = flowInstanceApp;
            _quotation = quotationApp;
            _workbenchApp = workbenchApp;
        }
    }
}