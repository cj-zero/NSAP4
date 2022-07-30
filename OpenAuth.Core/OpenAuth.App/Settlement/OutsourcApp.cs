using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Const;
using Infrastructure.Extensions;
using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAuth.App.Interface;
using OpenAuth.App.Material;
using OpenAuth.App.Material.Request;
using OpenAuth.App.Reponse;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Serve.Response;
using OpenAuth.App.Settlement.Request;
using OpenAuth.App.Workbench;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.Repository.Domain.Settlement;
using OpenAuth.Repository.Domain.Workbench;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class OutsourcApp : OnlyUnitWorkBaeApp
    {
        private readonly FlowInstanceApp _flowInstanceApp;
        private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;
        private readonly WorkbenchApp _workbenchApp;
        private readonly PendingApp _pendingApp;
        private readonly QuotationApp _quotationApp;
        private readonly UserManagerApp _userManagerApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public async Task<TableData> Load(QueryoutsourcListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //List<int?> outsourcIds = new List<int?>();
            List<int> serviceOrderId = new List<int>();
            if (!string.IsNullOrWhiteSpace(request.CompletionStartTime.ToString()) || !string.IsNullOrWhiteSpace(request.CompletionEndTime.ToString()))
            {
                //var completion = await UnitWork.Find<CompletionReport>(c => c.IsReimburse == 4)
                //    .WhereIf(!string.IsNullOrWhiteSpace(request.CompletionStartTime.ToString()), c => c.EndDate > request.CompletionStartTime)
                //    .WhereIf(!string.IsNullOrWhiteSpace(request.CompletionEndTime.ToString()), c => c.EndDate < Convert.ToDateTime(request.CompletionEndTime).AddDays(1))
                //    .Select(c => c.ServiceOrderId)
                //    .ToListAsync();
                var serviceOrder=await UnitWork.Find<ServiceWorkOrder>(null)
                    .WhereIf(!string.IsNullOrWhiteSpace(request.CompletionStartTime.ToString()), c => c.CompleteDate > request.CompletionStartTime)
                    .WhereIf(!string.IsNullOrWhiteSpace(request.CompletionEndTime.ToString()), c => c.CompleteDate < Convert.ToDateTime(request.CompletionEndTime).AddDays(1))
                    .Select(c => c.ServiceOrderId)
                    .ToListAsync();
                serviceOrderId.AddRange(serviceOrder);
            }

            var outsourcIds = await UnitWork.Find<OutsourcExpenses>(null)
                .WhereIf(serviceOrderId.Count > 0, o => serviceOrderId.Contains(o.ServiceOrderId.Value))
                .WhereIf(!string.IsNullOrWhiteSpace(request.ServiceOrderSapId), o => o.ServiceOrderSapId == int.Parse(request.ServiceOrderSapId))
                .WhereIf(!string.IsNullOrWhiteSpace(request.Customer), o => o.TerminalCustomer.Contains(request.Customer) || o.TerminalCustomerId.Contains(request.Customer))
                .Select(c => c.OutsourcId)
                .Distinct()
                .ToListAsync();

            var result = new TableData();
            var query = UnitWork.Find<Outsourc>(null).Include(c => c.OutsourcExpenses)
                        .WhereIf(!string.IsNullOrWhiteSpace(request.CreateName), q => q.CreateUser.Contains(request.CreateName))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.OutsourcId), q => q.Id == int.Parse(request.OutsourcId))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.StartTime.ToString()), q => q.CreateTime > request.StartTime)
                       .WhereIf(!string.IsNullOrWhiteSpace(request.EndTime.ToString()), q => q.CreateTime < Convert.ToDateTime(request.EndTime).AddDays(1))
                       .Where(o => outsourcIds.Contains(o.Id));

            //主页报表跳转用
            if (!string.IsNullOrWhiteSpace(request.StatusType))
            {
                var scheme = await UnitWork.Find<FlowScheme>(c => c.SchemeName == "个人代理结算").Select(c => c.Id).FirstOrDefaultAsync();
                List<string> ids = new List<string>();
                switch (request.StatusType)
                {
                    case "1":
                        ids = await UnitWork.Find<FlowInstance>(c => c.SchemeId == scheme && c.ActivityName != "财务支付" && c.ActivityName != "结束").Select(c => c.Id).ToListAsync();
                        query = query.Where(r => ids.Contains(r.FlowInstanceId));//待审核
                        break;
                    case "2":
                        ids = await UnitWork.Find<FlowInstance>(c => c.SchemeId == scheme && c.ActivityName == "结束").Select(c => c.Id).ToListAsync();
                        query = query.Where(r => ids.Contains(r.FlowInstanceId));//已支付
                        break;
                    case "3":
                        ids = await UnitWork.Find<FlowInstance>(c => c.SchemeId == scheme && c.ActivityName == "财务支付").Select(c => c.Id).ToListAsync();
                        query = query.Where(r => ids.Contains(r.FlowInstanceId));//待支付
                        break;
                }
            }

            if (loginContext.User.Account != Define.SYSTEM_USERNAME)
            {
                #region 筛选条件
                //var schemeContent = await .FirstOrDefaultAsync();
                List<string> Lines = new List<string>();
                List<string> flowInstanceIds = new List<string>();
                var lineId = "";
                var SchemeContent = await UnitWork.Find<FlowScheme>(f => f.SchemeName.Equals("个人代理结算")).Select(f => f.SchemeContent).FirstOrDefaultAsync();
                SchemeContentJson schemeJson = JsonHelper.Instance.Deserialize<SchemeContentJson>(SchemeContent);
                if (request.PageType != null && request.PageType > 0)
                {
                    if (loginContext.Roles.Any(r => r.Name.Equals("客服主管")))
                    {
                        lineId = schemeJson.Nodes.Where(n => n.name.Equals("客服主管审批")).FirstOrDefault()?.id;
                    }
                    else if (loginContext.Roles.Any(r => r.Name.Equals("总经理")))
                    {
                        lineId = schemeJson.Nodes.Where(n => n.name.Equals("总经理审批")).FirstOrDefault()?.id;
                    }
                    else if (loginContext.Roles.Any(r => r.Name.Equals("出纳")))
                    {
                        lineId = schemeJson.Nodes.Where(n => n.name.Equals("财务支付")).FirstOrDefault()?.id;
                    }
                }
                switch (request.PageType)
                {
                    case 1:

                        Lines.Add(lineId);
                        break;
                    case 2:
                        List<string> lineIds = new List<string>();
                        var lineIdTo = lineId;
                        foreach (var item in schemeJson.Lines)
                        {
                            if (schemeJson.Lines.Where(l => l.from.Equals(lineIdTo)).FirstOrDefault()?.to != null)
                            {
                                lineIdTo = schemeJson.Lines.Where(l => l.from.Equals(lineIdTo)).FirstOrDefault()?.to;
                                lineIds.Add(lineIdTo);
                            }
                            else
                            {
                                break;
                            }
                        }
                        Lines.AddRange(lineIds);
                        break;
                    case 3:
                        Lines.Add(lineId);
                        break;
                    case 4:
                        if (loginContext.Roles.Any(r => r.Name.Equals("出纳")))
                        {
                            Lines.Add(schemeJson.Nodes.Where(n => n.name.Equals("结束")).FirstOrDefault()?.id);
                        }
                        break;
                    default:
                        var orgRole = await UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.Key == Define.ORGROLE && c.FirstId == loginContext.User.Id).FirstOrDefaultAsync();
                        if (orgRole != null)//查看本部下数据
                        {
                            var orgId = orgRole.SecondId;
                            var userId = await UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.SecondId == orgId && c.Key == Define.USERORG).Select(c => c.FirstId).ToListAsync();
                            query = query.Where(r => userId.Contains(r.CreateUserId));
                        }
                        else
                        {
                            query = query.Where(q => q.CreateUserId.Equals(loginContext.User.Id));
                        }
                        break;
                }
                if (Lines.Count > 0)
                {
                    flowInstanceIds = await UnitWork.Find<FlowInstance>(f => Lines.Contains(f.ActivityId)).Select(s => s.Id).ToListAsync();
                    query = query.Where(q => flowInstanceIds.Contains(q.FlowInstanceId));
                }

                #endregion
            }

            var outsourcList = await query.OrderByDescending(o => o.UpdateTime).Skip((request.page - 1) * request.limit).Take(request.limit).ToListAsync();
            var serviceOrderIds = outsourcList.Select(o => o.OutsourcExpenses.FirstOrDefault()?.ServiceOrderId).ToList();
            var serviceWorkOrder = await UnitWork.Find<ServiceWorkOrder>(s => serviceOrderIds.Contains(s.ServiceOrderId)).ToListAsync();
            var flowInstanceList = await UnitWork.Find<FlowInstance>(f => outsourcList.Select(o => o.FlowInstanceId).ToList().Contains(f.Id)).ToListAsync();
            result.Count = await query.CountAsync();

            var sumMoney = await query.Where(a => a.TotalMoney > 0).SumAsync(a => a.TotalMoney);

            var userIds = outsourcList.Select(o => o.CreateUserId).ToList();
            var SelOrgName = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(null).Select(o => new { o.Id, o.Name, o.CascadeId }).ToListAsync();
            var Relevances = await UnitWork.Find<Relevance>(r => r.Key == Define.USERORG && userIds.Contains(r.FirstId)).Select(r => new { r.FirstId, r.SecondId }).ToListAsync();

            List<dynamic> outsourcs = new List<dynamic>();
            outsourcList.ForEach(o =>
            {
                var orgName = SelOrgName.Where(s => s.Id.Equals(Relevances.Where(r => r.FirstId.Equals(o.CreateUserId)).FirstOrDefault()?.SecondId)).FirstOrDefault()?.Name;
                var outsourcexpensesObj = o.OutsourcExpenses.FirstOrDefault();
                var serviceWorkOrderObj = serviceWorkOrder.Where(s => s.ServiceOrderId == outsourcexpensesObj?.ServiceOrderId && s.CurrentUserNsapId.Equals(o.CreateUserId)).FirstOrDefault();
                outsourcs.Add(new
                {
                    o.Id,
                    o.ServiceMode,
                    UpdateTime = Convert.ToDateTime(o.UpdateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                    CreateTime = Convert.ToDateTime(o.CreateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                    outsourcexpensesObj?.ServiceOrderSapId,
                    outsourcexpensesObj?.TerminalCustomer,
                    outsourcexpensesObj?.TerminalCustomerId,
                    serviceWorkOrderObj?.FromTheme,
                    serviceWorkOrderObj?.ManufacturerSerialNumber,
                    serviceWorkOrderObj?.MaterialCode,
                    StatusName = o.FlowInstanceId == null ? "未提交" : flowInstanceList.Where(f => f.Id.Equals(o.FlowInstanceId)).FirstOrDefault()?.IsFinish == FlowInstanceStatus.Rejected ? "驳回" : flowInstanceList.Where(f => f.Id.Equals(o.FlowInstanceId)).FirstOrDefault()?.ActivityName == "开始" ? "未提交" : flowInstanceList.Where(f => f.Id.Equals(o.FlowInstanceId)).FirstOrDefault()?.ActivityName == "结束" ? "已支付" : flowInstanceList.Where(f => f.Id.Equals(o.FlowInstanceId)).FirstOrDefault()?.ActivityName,
                    PayTime = o.PayTime != null ? Convert.ToDateTime(o.PayTime).ToString("yyyy.MM.dd HH:mm:ss") : null,
                    o.TotalMoney,
                    CreateUser = orgName == null ? o.CreateUser : orgName + "-" + o.CreateUser,
                    o.Remark,
                    IsRejected = o.IsRejected ? "是" : null
                });
            });
            result.Data = new
            {
                data = outsourcs,
                sumMoney = sumMoney
            };
            return result;
        }

        /// <summary>
        /// 结算费用归属
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> LoadCostAttribution(QueryoutsourcListReq request)
        {
            request.SelectMode = 2;
            return await CostAttribution(request);
        }

        /// <summary>
        /// 主管查看费用归属报表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> AnalysisReportCostManager(QueryoutsourcListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginOrg = loginContext.Orgs.OrderByDescending(o => o.CascadeId).FirstOrDefault();

            List<int?> serviceOrderId = new List<int?>();
            List<string> expendsId = new List<string>();
            List<OutsourcExpenseOrg> outsourcExpenseOrg = null;
            if (!string.IsNullOrWhiteSpace(request.CompletionStartTime.ToString()) || !string.IsNullOrWhiteSpace(request.CompletionEndTime.ToString()))
            {
                var completion = await UnitWork.Find<CompletionReport>(c => c.IsReimburse == 4)
                    .WhereIf(!string.IsNullOrWhiteSpace(request.CompletionStartTime.ToString()), c => c.EndDate > request.CompletionStartTime)
                    .WhereIf(!string.IsNullOrWhiteSpace(request.CompletionEndTime.ToString()), c => c.EndDate < Convert.ToDateTime(request.CompletionEndTime).AddDays(1))
                    .Select(c => c.ServiceOrderId)
                    .ToListAsync();
                serviceOrderId.AddRange(completion);
            }
            if (request.PageType == 1)//主管查看
            {
                //归在该部门下的费用
                outsourcExpenseOrg = await UnitWork.Find<OutsourcExpenseOrg>(c => c.OrgId == loginOrg.Id).ToListAsync();
                expendsId.AddRange(outsourcExpenseOrg.Select(c => c.ExpenseId).ToList());
            }

            var outsourcIds = await UnitWork.Find<OutsourcExpenses>(null)
                .WhereIf(expendsId.Count > 0, o => expendsId.Contains(o.Id))
                .WhereIf(serviceOrderId.Count > 0, o => serviceOrderId.Contains(o.ServiceOrderId))
                .WhereIf(!string.IsNullOrWhiteSpace(request.ServiceOrderSapId), o => o.ServiceOrderSapId == int.Parse(request.ServiceOrderSapId))
                .WhereIf(!string.IsNullOrWhiteSpace(request.Customer), o => o.TerminalCustomer.Contains(request.Customer) || o.TerminalCustomerId.Contains(request.Customer))
                .Select(c => c.OutsourcId)
                .Distinct()
                .ToListAsync();

            var result = new TableData();
            var query = UnitWork.Find<Outsourc>(null).Include(c => c.OutsourcExpenses)
                        .WhereIf(!string.IsNullOrWhiteSpace(request.CreateName), q => q.CreateUser.Contains(request.CreateName))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.OutsourcId), q => q.Id == int.Parse(request.OutsourcId))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.StartTime.ToString()), q => q.CreateTime > request.StartTime)
                       .WhereIf(!string.IsNullOrWhiteSpace(request.EndTime.ToString()), q => q.CreateTime < Convert.ToDateTime(request.EndTime).AddDays(1))
                       .Where(o => outsourcIds.Contains(o.Id));

            #region 取客服主管审批后的单
            var SchemeContent = await UnitWork.Find<FlowScheme>(f => f.SchemeName.Equals("个人代理结算")).Select(f => f.SchemeContent).FirstOrDefaultAsync();
            SchemeContentJson schemeJson = JsonHelper.Instance.Deserialize<SchemeContentJson>(SchemeContent);
            var lineId = schemeJson.Nodes.Where(n => n.name.Equals("客服主管审批")).FirstOrDefault()?.id;
            List<string> lineIds = new List<string>();
            List<string> Lines = new List<string>();
            List<string> flowInstanceIds = new List<string>();
            var lineIdTo = lineId;
            foreach (var item in schemeJson.Lines)
            {
                if (schemeJson.Lines.Where(l => l.from.Equals(lineIdTo)).FirstOrDefault()?.to != null)
                {
                    lineIdTo = schemeJson.Lines.Where(l => l.from.Equals(lineIdTo)).FirstOrDefault()?.to;
                    lineIds.Add(lineIdTo);
                }
                else
                {
                    break;
                }
            }
            Lines.AddRange(lineIds);
            if (Lines.Count > 0)
            {
                flowInstanceIds = await UnitWork.Find<FlowInstance>(f => Lines.Contains(f.ActivityId)).Select(s => s.Id).ToListAsync();
                query = query.Where(q => flowInstanceIds.Contains(q.FlowInstanceId));
            }
            #endregion

            var outsourcList = await query.OrderByDescending(o => o.UpdateTime).Skip((request.page - 1) * request.limit).Take(request.limit).ToListAsync();
            var serviceOrderIds = outsourcList.Select(o => o.OutsourcExpenses.FirstOrDefault()?.ServiceOrderId).ToList();
            var serviceWorkOrder = await UnitWork.Find<ServiceWorkOrder>(s => serviceOrderIds.Contains(s.ServiceOrderId)).ToListAsync();
            var flowInstanceList = await UnitWork.Find<FlowInstance>(f => outsourcList.Select(o => o.FlowInstanceId).ToList().Contains(f.Id)).ToListAsync();
            result.Count = await query.CountAsync();
            var userIds = outsourcList.Select(o => o.CreateUserId).ToList();
            var SelOrgName = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(null).Select(o => new { o.Id, o.Name, o.CascadeId }).ToListAsync();
            var Relevances = await UnitWork.Find<Relevance>(r => r.Key == Define.USERORG && userIds.Contains(r.FirstId)).Select(r => new { r.FirstId, r.SecondId }).ToListAsync();

            List<OutsourceResp> outsourcs = new List<OutsourceResp>();
            outsourcList.ForEach(o =>
            {
                var orgName = SelOrgName.Where(s => s.Id.Equals(Relevances.Where(r => r.FirstId.Equals(o.CreateUserId)).FirstOrDefault()?.SecondId)).FirstOrDefault()?.Name;
                var outsourcexpensesObj = o.OutsourcExpenses.FirstOrDefault();
                var serviceWorkOrderObj = serviceWorkOrder.Where(s => s.ServiceOrderId == outsourcexpensesObj?.ServiceOrderId && s.CurrentUserNsapId.Equals(o.CreateUserId)).FirstOrDefault();
                decimal? money = null;
                if (outsourcExpenseOrg != null)//不是查看全部
                {
                    o.OutsourcExpenses.ForEach(e =>
                    {
                        var org = outsourcExpenseOrg.Where(u => u.ExpenseId == e.Id && u.OrgId == loginOrg.Id).FirstOrDefault();
                        if (org != null)
                        {
                            money += e.Money * (org.Ratio / 100);
                        }
                    });
                }
                outsourcs.Add(new OutsourceResp
                {
                    CostOrgMoney = money,
                    TerminalCustomer=outsourcexpensesObj?.TerminalCustomer,
                    TerminalCustomerId=outsourcexpensesObj?.TerminalCustomerId,
                    FromTheme=serviceWorkOrderObj?.FromTheme,
                    TotalMoney=o.TotalMoney,
                    CreateUser = orgName == null ? o.CreateUser : orgName + "-" + o.CreateUser,
                });
            });

            List<AnalysisReportResp> AnalysisReportRespList = new List<AnalysisReportResp>();
            var user= outsourcs.GroupBy(c => c.CreateUser).Select(c => new AnalysisReportSublist { Name = c.Key, Count = c.Count(), TotalMoney = c.Sum(s => s.TotalMoney) }).OrderByDescending(c => c.TotalMoney).ToList();
            AnalysisReportRespList.Add(new AnalysisReportResp { Name = "User", AnalysisReportSublists = user });
            var customer = outsourcs.GroupBy(c => c.TerminalCustomer).Select(c => new AnalysisReportSublist { Name = c.Key, Count = c.Count(), TotalMoney = c.Sum(s => s.TotalMoney) }).OrderByDescending(c => c.TotalMoney).ToList();
            AnalysisReportRespList.Add(new AnalysisReportResp { Name = "Customer", AnalysisReportSublists = customer });

            result.Data = AnalysisReportRespList;
            return result;
        }

        /// <summary>
        /// 获取费用归属总金额
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> GetMoney(QueryoutsourcListReq request)
        {
            request.SelectMode = 1;
            TableData result = new TableData();
            result.Data= await CostAttribution(request);
            return result;
        }
        /// <summary>
        /// 费用归属 结算
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<dynamic> CostAttribution(QueryoutsourcListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginOrg = loginContext.Orgs.OrderByDescending(o => o.CascadeId).FirstOrDefault();

            List<int> serviceOrderId = new List<int>();
            List<string> expendsId = new List<string>();
            List<OutsourcExpenseOrg> outsourcExpenseOrg = null;
            var CompletionReports = await UnitWork.Find<CompletionReport>(c => c.IsReimburse == 4)
                    .WhereIf(!string.IsNullOrWhiteSpace(request.CompletionStartTime.ToString()), c => c.EndDate > request.CompletionStartTime)
                    .WhereIf(!string.IsNullOrWhiteSpace(request.CompletionEndTime.ToString()), c => c.EndDate < Convert.ToDateTime(request.CompletionEndTime).AddDays(1))
                    .WhereIf(!string.IsNullOrWhiteSpace(request.FromTheme), c => c.FromTheme.Contains(request.FromTheme))
                    .ToListAsync();
            //if (!string.IsNullOrWhiteSpace(request.CompletionStartTime.ToString()) || !string.IsNullOrWhiteSpace(request.CompletionEndTime.ToString()))
            //{
            //    var completion = await UnitWork.Find<CompletionReport>(c => c.IsReimburse == 4)
            //        .WhereIf(!string.IsNullOrWhiteSpace(request.CompletionStartTime.ToString()), c => c.EndDate > request.CompletionStartTime)
            //        .WhereIf(!string.IsNullOrWhiteSpace(request.CompletionEndTime.ToString()), c => c.EndDate < Convert.ToDateTime(request.CompletionEndTime).AddDays(1))
            //        .Select(c => c.ServiceOrderId.Value)
            //        .ToListAsync();
            //    serviceOrderId.AddRange(completion);
            //}
            //if (!string.IsNullOrWhiteSpace(request.FromTheme))
            //{
            //    var ids = await UnitWork.Find<ServiceWorkOrder>(c => c.FromTheme.Contains(request.FromTheme)).Select(c => c.ServiceOrderId).Distinct().ToListAsync();
            //    serviceOrderId = serviceOrderId.Count > 0 ? serviceOrderId.Intersect(ids).Distinct().ToList() : ids;
            //}
            serviceOrderId.AddRange(CompletionReports.Select(c => c.ServiceOrderId.Value).ToList());
            //int power = 0;
            if (request.PageType==1)//主管查看
            {
                //归在该部门下的费用
                outsourcExpenseOrg = await UnitWork.Find<OutsourcExpenseOrg>(c => c.OrgId == loginOrg.Id).ToListAsync();
                expendsId.AddRange(outsourcExpenseOrg.Select(c => c.ExpenseId).ToList());
            }
            //if (!loginContext.Roles.Any(r => r.Name.Equals("费用归属-呼叫中心")) && loginContext.User.Account != Define.SYSTEM_USERNAME)
            //{
            //    power = 1;
            //}

            var outsourcIds = await UnitWork.Find<OutsourcExpenses>(null)
                .WhereIf(request.PageType == 1, o => expendsId.Contains(o.Id))
                .WhereIf(serviceOrderId.Count > 0, o => serviceOrderId.Contains(o.ServiceOrderId.Value))
                .WhereIf(!string.IsNullOrWhiteSpace(request.ServiceOrderSapId), o => o.ServiceOrderSapId == int.Parse(request.ServiceOrderSapId))
                .WhereIf(!string.IsNullOrWhiteSpace(request.Customer), o => o.TerminalCustomer.Contains(request.Customer) || o.TerminalCustomerId.Contains(request.Customer))
                .Select(c => c.OutsourcId)
                .Distinct()
                .ToListAsync();

            var result = new TableData();
            var query = UnitWork.Find<Outsourc>(null).Where(c => c.Id >= 285).Include(c => c.OutsourcExpenses)//285之后才有费用归属
                        .WhereIf(!string.IsNullOrWhiteSpace(request.CreateName), q => q.CreateUser.Contains(request.CreateName))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.OutsourcId), q => q.Id == int.Parse(request.OutsourcId))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.StartTime.ToString()), q => q.CreateTime > request.StartTime)
                       .WhereIf(!string.IsNullOrWhiteSpace(request.EndTime.ToString()), q => q.CreateTime < Convert.ToDateTime(request.EndTime).AddDays(1))
                       .Where(o => outsourcIds.Contains(o.Id));

            #region 取客服主管审批后的单
            var SchemeContent = await UnitWork.Find<FlowScheme>(f => f.SchemeName.Equals("个人代理结算")).Select(f => f.SchemeContent).FirstOrDefaultAsync();
            SchemeContentJson schemeJson = JsonHelper.Instance.Deserialize<SchemeContentJson>(SchemeContent);
            var lineId = schemeJson.Nodes.Where(n => n.name.Equals("客服主管审批")).FirstOrDefault()?.id;
            List<string> lineIds = new List<string>();
            List<string> Lines = new List<string>();
            List<string> flowInstanceIds = new List<string>();
            var lineIdTo = lineId;
            foreach (var item in schemeJson.Lines)
            {
                if (schemeJson.Lines.Where(l => l.from.Equals(lineIdTo)).FirstOrDefault()?.to != null)
                {
                    lineIdTo = schemeJson.Lines.Where(l => l.from.Equals(lineIdTo)).FirstOrDefault()?.to;
                    lineIds.Add(lineIdTo);
                }
                else
                {
                    break;
                }
            }
            Lines.AddRange(lineIds);
            if (Lines.Count > 0)
            {
                flowInstanceIds = await UnitWork.Find<FlowInstance>(f => Lines.Contains(f.ActivityId)).Select(s => s.Id).ToListAsync();
                query = query.Where(q => flowInstanceIds.Contains(q.FlowInstanceId));
            }
            #endregion

            if (request.SelectMode==1)//查询总金额
            {
                var outsourcList = await query.ToListAsync();
                decimal? money = 0;
                if (request.PageType == 2)//查看全部数据
                {
                    money = outsourcList.Sum(c => c.TotalMoney);
                }
                else//查看部门数据
                {
                    outsourcList.ForEach(o =>
                    {
                        if (outsourcExpenseOrg != null)//不是查看全部
                        {
                            o.OutsourcExpenses.ForEach(e =>
                            {
                                //
                                var org = outsourcExpenseOrg.Where(u => u.ExpenseId == e.Id && u.OrgId == loginOrg.Id).FirstOrDefault();
                                if (org != null)
                                {
                                    money += e.Money * (org.Ratio / 100);
                                }
                            });
                        }
                    });
                }
                //result.Data = money;
                return money;
            }
            else
            {
                //var outsourcList = await query.ToListAsync();
                var outsourcList = await query.OrderByDescending(o => o.UpdateTime).Skip((request.page - 1) * request.limit).Take(request.limit).ToListAsync();
                var serviceOrderIds = outsourcList.Select(o => o.OutsourcExpenses.FirstOrDefault()?.ServiceOrderId).ToList();
                //var serviceWorkOrder = await UnitWork.Find<ServiceWorkOrder>(s => serviceOrderIds.Contains(s.ServiceOrderId)).WhereIf(!string.IsNullOrWhiteSpace(request.FromTheme), c => c.FromTheme.Contains(request.FromTheme)).ToListAsync();
                var flowInstanceList = await UnitWork.Find<FlowInstance>(f => outsourcList.Select(o => o.FlowInstanceId).ToList().Contains(f.Id)).ToListAsync();
                result.Count = await query.CountAsync();
                var userIds = outsourcList.Select(o => o.CreateUserId).ToList();
                var SelOrgName = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(null).Select(o => new { o.Id, o.Name, o.CascadeId }).ToListAsync();
                var Relevances = await UnitWork.Find<Relevance>(r => r.Key == Define.USERORG && userIds.Contains(r.FirstId)).Select(r => new { r.FirstId, r.SecondId }).ToListAsync();

                List<dynamic> outsourcs = new List<dynamic>();
                outsourcList.ForEach(o =>
                {
                    var orgName = SelOrgName.Where(s => s.Id.Equals(Relevances.Where(r => r.FirstId.Equals(o.CreateUserId)).FirstOrDefault()?.SecondId)).FirstOrDefault()?.Name;
                    var outsourcexpensesObj = o.OutsourcExpenses.FirstOrDefault();
                    //var serviceWorkOrderObj = serviceWorkOrder.Where(s => s.ServiceOrderId == outsourcexpensesObj?.ServiceOrderId && s.CurrentUserNsapId.Equals(o.CreateUserId)).FirstOrDefault();
                    var serviceWorkOrderObj = CompletionReports.Where(s => s.ServiceOrderId == outsourcexpensesObj?.ServiceOrderId && s.CreateUserId.Equals(o.CreateUserId)).FirstOrDefault();
                    decimal? money = null;
                    if (outsourcExpenseOrg != null)//不是查看全部
                    {
                        o.OutsourcExpenses.ForEach(e =>
                        {
                            var org = outsourcExpenseOrg.Where(u => u.ExpenseId == e.Id && u.OrgId == loginOrg.Id).FirstOrDefault();
                            if (org != null)
                            {
                                money += e.Money * (org.Ratio / 100);
                            }
                        });
                    }
                    outsourcs.Add(new
                    {
                        o.Id,
                        o.ServiceMode,
                        CostOrgMoney = money,
                        UpdateTime = Convert.ToDateTime(o.UpdateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                        CreateTime = Convert.ToDateTime(o.CreateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                        outsourcexpensesObj?.ServiceOrderSapId,
                        outsourcexpensesObj?.TerminalCustomer,
                        outsourcexpensesObj?.TerminalCustomerId,
                        serviceWorkOrderObj?.FromTheme,
                        serviceWorkOrderObj?.ManufacturerSerialNumber,
                        serviceWorkOrderObj?.MaterialCode,
                        StatusName = o.FlowInstanceId == null ? "未提交" : flowInstanceList.Where(f => f.Id.Equals(o.FlowInstanceId)).FirstOrDefault()?.IsFinish == FlowInstanceStatus.Rejected ? "驳回" : flowInstanceList.Where(f => f.Id.Equals(o.FlowInstanceId)).FirstOrDefault()?.ActivityName == "开始" ? "未提交" : flowInstanceList.Where(f => f.Id.Equals(o.FlowInstanceId)).FirstOrDefault()?.ActivityName == "结束" ? "已支付" : flowInstanceList.Where(f => f.Id.Equals(o.FlowInstanceId)).FirstOrDefault()?.ActivityName,
                        PayTime = o.PayTime != null ? Convert.ToDateTime(o.PayTime).ToString("yyyy.MM.dd HH:mm:ss") : null,
                        o.TotalMoney,
                        CreateUser = orgName == null ? o.CreateUser : orgName + "-" + o.CreateUser,
                        o.Remark,
                        IsRejected = o.IsRejected ? "是" : null
                    });
                });
                result.Data = outsourcs;
                return result;
            }
            //return result;
        }

        /// <summary>
        /// 获取所有需要结算服务单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> GetServiceOrder(QueryoutsourcListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            List<int> serviceOrderIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(request.OutsourcId))
            {
                serviceOrderIds = await UnitWork.Find<OutsourcExpenses>(o => o.OutsourcId == int.Parse(request.OutsourcId)).Select(o => (int)o.ServiceOrderId).ToListAsync();
            }
            //var serviceOrderIds = await UnitWork.Find<OutsourcExpenses>(null).Select(o => (int)o.ServiceOrderId).ToListAsync();
            var query = from a in UnitWork.Find<CompletionReport>(c => c.CreateUserId.Equals(loginContext.User.Id) && (c.IsReimburse <= 1 || serviceOrderIds.Contains((int)c.ServiceOrderId)))
                        join b in UnitWork.Find<ServiceOrder>(c => c.VestInOrg == 1).Include(s => s.ServiceWorkOrders) on a.ServiceOrderId equals b.Id into ab
                        from b in ab.DefaultIfEmpty()
                            //where !serviceOrderIds.Contains(b.Id)
                        select new { a, b };
            //&& ((c.CreateTime.Value.Year == DateTime.Now.Year) || (c.CreateTime.Value.Year == DateTime.Now.Year - 1 && c.CreateTime.Value.Month == 12 && DateTime.Now.Month == 1))
            if (!string.IsNullOrWhiteSpace(request.Month.ToString()))
            {
                query = query.Where(c => c.a.CreateTime.Value.Month == request.Month);
            }
            else
            {
                query = query.WhereIf((bool)request.IsMonth, c => c.a.CreateTime.Value.Month == DateTime.Now.Month && c.a.CreateTime.Value.Year == DateTime.Now.Year);
                if (DateTime.Now.Month == 1)
                {
                    query = query.WhereIf(!(bool)request.IsMonth, c => c.a.CreateTime.Value.Month == 12 && c.a.CreateTime.Value.Year == DateTime.Now.Year - 1);
                }
                else
                {
                    query = query.WhereIf(!(bool)request.IsMonth, c => c.a.CreateTime.Value.Month == DateTime.Now.Month - 1 && c.a.CreateTime.Value.Year == DateTime.Now.Year);
                }
            }
            var serviceOrderList = await query.Select(q => new { q.b.Id, q.b.U_SAP_ID, q.b.TerminalCustomer, q.b.TerminalCustomerId, q.b.ServiceWorkOrders, q.a.CreateTime }).OrderByDescending(u => u.Id).ToListAsync();
            serviceOrderList = serviceOrderList.GroupBy(s => s.U_SAP_ID).Select(s => s.First()).ToList();
            List<dynamic> objs = new List<dynamic>();
            serviceOrderIds = serviceOrderList.Select(s => s.Id).ToList();
            var serviceEvaluate = await UnitWork.Find<ServiceEvaluate>(s => serviceOrderIds.Contains((int)s.ServiceOrderId)).ToListAsync();
            serviceOrderList.ForEach(s =>
            {
                var count = s.ServiceWorkOrders.Where(w => (w.Status < 7 || w.ServiceMode != request.ServiceMode) && w.CurrentUserNsapId == loginContext.User.Id).Count();
                count += serviceEvaluate.Where(s => (s.ServicePrice + s.ServiceAttitude + s.SchemeEffectiveness + s.ProductQuality + s.ResponseSpeed) / 5 < 3).Count();
                if (count <= 0 && s.ServiceWorkOrders.Where(w => w.CurrentUserNsapId.Equals(loginContext.User.Id)).Count() > 0)
                {
                    var serviceWorkOrderObj = s.ServiceWorkOrders.FirstOrDefault();
                    objs.Add(new
                    {
                        ServiceOrderId = s.Id,
                        s.TerminalCustomer,
                        s.TerminalCustomerId,
                        CompleteDate = Convert.ToDateTime(s.CreateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                        ServiceOrderSapId = s.U_SAP_ID,
                        serviceWorkOrderObj?.FromTheme,
                        serviceWorkOrderObj?.ManufacturerSerialNumber,
                        serviceWorkOrderObj?.MaterialCode,
                        serviceWorkOrderObj?.Remark
                    });
                }
            });
            result.Data = objs.Skip((request.page - 1) * request.limit).Take(request.limit).ToList();
            result.Count = objs.Count();
            return result;
        }

        /// <summary>
        /// 获取服务单详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> ServiceOrderDetails(QueryoutsourcListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var serviceWorkOrderList = await UnitWork.Find<ServiceWorkOrder>(s => request.ServiceOrderIds.Contains(s.ServiceOrderId)).ToListAsync();
            var serviceDailyReportList = await UnitWork.Find<ServiceDailyReport>(s => request.ServiceOrderIds.Contains(s.ServiceOrderId)).ToListAsync();
            var baseInfo = new
            {
                ServiceWorkOrderList = serviceWorkOrderList.Select(s => new
                {
                    s.ManufacturerSerialNumber,
                    s.MaterialCode,
                    s.MaterialDescription,
                    s.FromTheme,
                    s.Remark,
                    s.CreateTime
                }).OrderBy(s => s.CreateTime).ToList(),
                ServiceDailyReportList = serviceDailyReportList.Select(s => new
                {
                    s.CreateTime,
                    s.ManufacturerSerialNumber,
                    s.MaterialCode,
                    ProcessCode = GetServiceTroubleAndSolution(s.ProcessDescription, "code"),
                    ProcessDescription = GetServiceTroubleAndSolution(s.ProcessDescription, "description"),
                    TroubleCode = GetServiceTroubleAndSolution(s.TroubleDescription, "code"),
                    TroubleDescription = GetServiceTroubleAndSolution(s.TroubleDescription, "description")
                }).OrderBy(s => s.CreateTime).ToList()
            };
            var money = 0;
            if (request.ServiceMode == 2)
            {
                var serviceOrderIds = await UnitWork.Find<ServiceOrder>(s => request.ServiceOrderIds.Contains(s.Id)).ToListAsync();
                var outsourcIds = await UnitWork.Find<Outsourc>(o => o.CreateUserId.Equals(loginContext.User.Id)).WhereIf(!string.IsNullOrWhiteSpace(request.OutsourcId), e => e.Id != int.Parse(request.OutsourcId)).Select(s => s.Id).ToListAsync();
                var thisMonth = await UnitWork.Find<OutsourcExpenses>(e => outsourcIds.Contains((int)e.OutsourcId) && e.CompleteTime.Value.Year == DateTime.Now.Year && e.CompleteTime.Value.Month == DateTime.Now.Month && e.ExpenseType == 4 && e.IsOverseas == false).CountAsync();
                var lastMonth = await UnitWork.Find<OutsourcExpenses>(e => outsourcIds.Contains((int)e.OutsourcId) && e.CompleteTime.Value.Year == DateTime.Now.Year && e.CompleteTime.Value.Month == DateTime.Now.Month - 1 && e.ExpenseType == 4 && e.IsOverseas == false).CountAsync();
                var number = 0;
                var completionReportList = await UnitWork.Find<CompletionReport>(c => request.ServiceOrderIds.Contains(c.ServiceOrderId)).ToListAsync();
                var globalarea = await UnitWork.Find<GlobalArea>(g => g.AreaLevel == "3" && g.Pid == "99").Select(g => g.AreaName).ToListAsync();
                request.ServiceOrderIds.ForEach(s =>
                {
                    var completionReportObj = completionReportList.Where(c => c.ServiceOrderId == s).OrderByDescending(c => c.CreateTime).FirstOrDefault();
                    if (completionReportObj.CreateTime.Value.Month == DateTime.Now.Month)
                    {
                        number = thisMonth++;
                    }
                    else if (completionReportObj.CreateTime.Value.Month == DateTime.Now.Month - 1)
                    {
                        number = lastMonth++;
                    }
                    var Province = serviceOrderIds.Where(o => o.Id == s).FirstOrDefault()?.Province;
                    if (globalarea.Contains(Province) || Province == "海外")
                    {
                        money += 50;
                    }
                    else
                    {
                        money += Calculation(number);
                    }

                });
                //result.Data = new { TotalMoney = money };
            }
            result.Data = new
            {
                baseInfo.ServiceWorkOrderList,
                baseInfo.ServiceDailyReportList,
                BaseInfo = new { TotalMoney = request.ServiceMode == 2 ? money : 0 }
            };
            return result;
        }

        /// <summary>
        /// 查询结算单详情
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetDetails(QueryoutsourcListReq req)
        {
            if (req.IsUpdate != null && (bool)req.IsUpdate)
            {
                var loginContext = _auth.GetCurrentUser();
                if (loginContext == null)
                {
                    throw new CommonException("登录已过期", Define.INVALID_TOKEN);
                }
                var result = new TableData();
                var outsourcObj = await UnitWork.Find<Outsourc>(o => o.Id == int.Parse(req.OutsourcId)).Include(o => o.OutsourcExpenses).ThenInclude(o => o.outsourcexpensespictures).FirstOrDefaultAsync();
                var History = await UnitWork.Find<FlowInstanceOperationHistory>(f => f.InstanceId.Equals(outsourcObj.FlowInstanceId)).OrderBy(f => f.CreateDate).ToListAsync();
                var StatusName = (await UnitWork.Find<FlowInstance>(f => outsourcObj.FlowInstanceId.Equals(f.Id)).FirstOrDefaultAsync())?.ActivityName;
                var OperationHistorys = History.Select(h => new
                {
                    CreateDate = Convert.ToDateTime(h.CreateDate).ToString("yyyy.MM.dd HH:mm:ss"),
                    h.Remark,
                    IntervalTime = h.IntervalTime != null && h.IntervalTime > 0 ? h.IntervalTime / 60 : null,
                    h.CreateUserName,
                    h.Content,
                    h.ApprovalResult,
                });
                var expenseIds = outsourcObj.OutsourcExpenses.Select(o => o.Id).ToList();
                var expenseOrgs = await UnitWork.Find<OutsourcExpenseOrg>(o => expenseIds.Contains(o.ExpenseId)).ToListAsync();
                var outsourcExpenses = outsourcObj.OutsourcExpenses.Select(o => new
                {
                    o.OutsourcId,
                    o.outsourcexpensespictures,
                    o.Money,
                    o.SerialNumber,
                    o.ServiceOrderId,
                    o.ServiceOrderSapId,
                    o.StartTime,
                    o.TerminalCustomer,
                    o.TerminalCustomerId,
                    o.To,
                    o.ToLat,
                    o.ToLng,
                    o.IsOverseas,
                    o.ManHour,
                    o.Id,
                    o.FromLng,
                    o.FromLat,
                    o.From,
                    o.ExpenseType,
                    o.EndTime,
                    o.Days,
                    o.CompleteTime,
                    OutsourcExpenseOrgs = expenseOrgs.Where(e => e.ExpenseId.Equals(o.Id)).ToList()
                });
                var serviceOrderObj = await UnitWork.Find<ServiceOrder>(s => s.Id == outsourcObj.OutsourcExpenses.FirstOrDefault().ServiceOrderId).Include(s => s.ServiceWorkOrders).FirstOrDefaultAsync();
                //为职员加上部门前缀
                var recepUserOrgInfo = await _userManagerApp.GetUserOrgInfo(serviceOrderObj.RecepUserId);
                serviceOrderObj.RecepUserDept = recepUserOrgInfo != null ? recepUserOrgInfo.OrgName : "";
                var salesManOrgInfo = await _userManagerApp.GetUserOrgInfo(serviceOrderObj.SalesManId);
                serviceOrderObj.SalesManDept = salesManOrgInfo != null ? salesManOrgInfo.OrgName : "";
                var superVisorOrgInfo = await _userManagerApp.GetUserOrgInfo(serviceOrderObj.SupervisorId);
                serviceOrderObj.SuperVisorDept = superVisorOrgInfo != null ? superVisorOrgInfo.OrgName : "";

                var serviceDailyReportList = await UnitWork.Find<ServiceDailyReport>(s => outsourcObj.OutsourcExpenses.FirstOrDefault().ServiceOrderId == s.ServiceOrderId).ToListAsync();
                var ocrd = await UnitWork.Find<OpenAuth.Repository.Domain.Sap.OCRD>(c => c.CardCode == serviceOrderObj.TerminalCustomerId).Select(c => new { c.Balance }).FirstOrDefaultAsync();
                var relevance = await UnitWork.Find<Relevance>(c => c.Key == Define.USERORG && c.FirstId == outsourcObj.CreateUserId).Select(c => c.SecondId).ToListAsync();
                var org = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(c => relevance.Contains(c.Id)).OrderByDescending(c => c.CascadeId).FirstOrDefaultAsync();
                result.Data = new
                {
                    BaseInfo = new
                    {
                        outsourcExpenses,
                        outsourcObj.OutsourcExpenses.FirstOrDefault()?.ServiceOrderId,
                        outsourcObj.OutsourcExpenses.FirstOrDefault()?.ServiceOrderSapId,
                        outsourcObj.Id,
                        serviceOrderObj.TerminalCustomer,
                        serviceOrderObj.TerminalCustomerId,
                        serviceOrderObj.SalesMan,
                        serviceOrderObj.SalesManDept,
                        serviceOrderObj.NewestContactTel,
                        serviceOrderObj.NewestContacter,
                        serviceOrderObj.Address,
                        serviceOrderObj.Supervisor,
                        serviceOrderObj.SuperVisorDept,
                        outsourcObj.CreateUser,
                        outsourcObj.ServiceMode,
                        StatusName,
                        OrgName = org?.Name,
                        Balance = ocrd?.Balance ?? 0m,
                    },
                    ServiceWorkOrderList = serviceOrderObj.ServiceWorkOrders.Where(s => s.CurrentUserNsapId.Equals(outsourcObj.CreateUserId)).Select(s => new
                    {
                        s.ManufacturerSerialNumber,
                        s.MaterialCode,
                        s.MaterialDescription,
                        s.FromTheme,
                        s.Remark,
                        CreateTime = Convert.ToDateTime(s.CreateTime).ToString("yyyy.MM.dd HH:mm:ss")
                    }).OrderBy(s => s.CreateTime).ToList(),
                    ServiceDailyReportList = serviceDailyReportList.Select(s => new
                    {
                        s.CreateTime,
                        s.ManufacturerSerialNumber,
                        s.MaterialCode,
                        ProcessCode = GetServiceTroubleAndSolution(s.ProcessDescription, "code"),
                        ProcessDescription = GetServiceTroubleAndSolution(s.ProcessDescription, "description"),
                        TroubleCode = GetServiceTroubleAndSolution(s.TroubleDescription, "code"),
                        TroubleDescription = GetServiceTroubleAndSolution(s.TroubleDescription, "description")
                    }).OrderBy(s => s.CreateTime).ToList(),
                    OperationHistorys
                };
                #region 废弃
                //if (outsourcObj.ServiceMode == 1)
                //{

                //}
                //else
                //{
                //    var servicerOrderIds = outsourcObj.OutsourcExpenses.Select(o => o.ServiceOrderId).ToList();
                //    var serviceWorkOrderList = await UnitWork.Find<ServiceWorkOrder>(s => servicerOrderIds.Contains(s.ServiceOrderId) && s.CurrentUserNsapId.Equals(outsourcObj.CreateUserId)).ToListAsync();
                //    serviceWorkOrderList = serviceWorkOrderList.GroupBy(s => s.ServiceOrderId).Select(s => s.FirstOrDefault()).ToList();
                //    result.Data = new
                //    {
                //        ServiceOrder = outsourcObj.OutsourcExpenses.Select(o => new
                //        {
                //            outsourcexpensesId = o.Id,
                //            o.ServiceOrderId,
                //            o.ServiceOrderSapId,
                //            o.TerminalCustomer,
                //            o.TerminalCustomerId,
                //            serviceWorkOrderList.Where(s => s.ServiceOrderId == o.ServiceOrderId).FirstOrDefault()?.FromTheme,
                //            serviceWorkOrderList.Where(s => s.ServiceOrderId == o.ServiceOrderId).FirstOrDefault()?.ManufacturerSerialNumber,
                //            serviceWorkOrderList.Where(s => s.ServiceOrderId == o.ServiceOrderId).FirstOrDefault()?.MaterialCode,
                //            serviceWorkOrderList.Where(s => s.ServiceOrderId == o.ServiceOrderId).FirstOrDefault()?.Remark
                //        }).OrderBy(s=>s.ServiceOrderSapId).ToList(),
                //        Month = outsourcObj.OutsourcExpenses.FirstOrDefault()?.CompleteTime.Value.Month,
                //        outsourcObj.ServiceMode,
                //        outsourcObj.Id,
                //        outsourcObj.TotalMoney,
                //        outsourcObj.Remark,
                //        outsourcObj.CreateUser,
                //        StatusName,
                //        OperationHistorys
                //    };
                //}
                #endregion

                return result;
            }
            else 
            {
                var workbenchPending = await UnitWork.Find<WorkbenchPending>(w => w.SourceNumbers == int.Parse(req.OutsourcId) && w.OrderType == 3).FirstOrDefaultAsync();
                Outsourc outsource = null;
                int? serviceOrderId = null;
                if (workbenchPending==null)//草稿状态
                {
                    outsource = await UnitWork.Find<Outsourc>(c => c.Id == int.Parse(req.OutsourcId)).Include(c => c.OutsourcExpenses).FirstOrDefaultAsync();
                    serviceOrderId = outsource.OutsourcExpenses.FirstOrDefault()?.ServiceOrderId;
                }
                var num = workbenchPending?.ApprovalNumber == null ? "0" : workbenchPending?.ApprovalNumber.ToString();
                return await _pendingApp.PendingDetails(new Workbench.Request.PendingReq { ApprovalNumber = num, SourceNumbers = req.OutsourcId, ServiceOrderId = serviceOrderId, Petitioner = outsource?.CreateUserId });
            }
           
        }

        /// <summary>
        /// 添加结算
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Add(AddOrUpdateoutsourcReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var obj = await Condition(req);
            obj.CreateTime = DateTime.Now;
            obj.CreateUser = loginUser.Name;
            obj.CreateUserId = loginUser.Id;
            var serviceOrderIds = obj.OutsourcExpenses.Select(s => s.ServiceOrderId).Distinct().ToList();
            obj.OutsourcExpenses.ForEach(o =>
            {
                o.Id = Guid.NewGuid().ToString();
                if (o.outsourcexpensespictures != null && o.outsourcexpensespictures.Count() > 0)
                {
                    o.outsourcexpensespictures.ForEach(p => p.Id = Guid.NewGuid().ToString());
                }
            });
            //事务保证数据一致
            var dbContext = UnitWork.GetDbContext<Outsourc>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var outsourcObj = await UnitWork.AddAsync<Outsourc, int>(obj);
                    await UnitWork.SaveAsync();
                    if (!req.IsDraft)
                    {
                        //创建结算流程
                        var mf = _moduleFlowSchemeApp.Get(m => m.Module.Name.Equals("个人代理结算"));
                        var afir = new AddFlowInstanceReq();
                        afir.SchemeId = mf.FlowSchemeId;
                        afir.FrmType = 2;
                        afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                        afir.CustomName = $"个人代理结算单";
                        afir.FrmData = "{\"ID\":\"" + outsourcObj.Id + "\"}";
                        afir.OrgId = loginContext.Orgs.OrderBy(o => o.CascadeId).FirstOrDefault()?.Id;
                        var FlowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(afir);
                        await UnitWork.UpdateAsync<Outsourc>(r => r.Id == outsourcObj.Id, r => new Outsourc { FlowInstanceId = FlowInstanceId });
                        //增加全局待处理
                        var serviceOrederObj = await UnitWork.Find<ServiceOrder>(s => s.Id == outsourcObj.OutsourcExpenses.FirstOrDefault().ServiceOrderId).FirstOrDefaultAsync();
                        await _workbenchApp.AddOrUpdate(new WorkbenchPending
                        {
                            OrderType = 3,
                            TerminalCustomer = serviceOrederObj.TerminalCustomer,
                            TerminalCustomerId = serviceOrederObj.TerminalCustomerId,
                            ServiceOrderId = serviceOrederObj.Id,
                            ServiceOrderSapId = (int)serviceOrederObj.U_SAP_ID,
                            UpdateTime = outsourcObj.UpdateTime,
                            Remark = outsourcObj.Remark,
                            FlowInstanceId = FlowInstanceId,
                            TotalMoney = outsourcObj.TotalMoney,
                            Petitioner = loginUser.Name,
                            SourceNumbers = outsourcObj.Id,
                            PetitionerId = loginUser.Id
                        });
                    }
                    await UnitWork.UpdateAsync<CompletionReport>(c => serviceOrderIds.Contains(c.ServiceOrderId) && c.CreateUserId.Equals(loginUser.Id), c => new CompletionReport { IsReimburse = 4 });
                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("添加结算单失败。请重试" + ex.Message);
                }

            }

        }
        /// <summary>
        /// 修改结算
        /// </summary>
        /// <param name="req"></param>
        public async Task Update(AddOrUpdateoutsourcReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var obj = await Condition(req);
            obj.Id = (int)req.outsourcId;
            var dbContext = UnitWork.GetDbContext<Outsourc>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var outsourcObj = await UnitWork.Find<Outsourc>(o => o.Id == req.outsourcId).Include(o => o.OutsourcExpenses).ThenInclude(o => o.outsourcexpensespictures).FirstOrDefaultAsync();
                    #region 删除重新新增
                    List<OutsourcExpensesPicture> pictureList = new List<OutsourcExpensesPicture>();
                    outsourcObj.OutsourcExpenses.ForEach(o => pictureList.AddRange(o.outsourcexpensespictures));
                    await UnitWork.BatchDeleteAsync<OutsourcExpensesPicture>(pictureList.ToArray());
                    await UnitWork.BatchDeleteAsync<OutsourcExpenses>(outsourcObj.OutsourcExpenses.ToArray());
                    await UnitWork.SaveAsync();
                    obj.OutsourcExpenses.ForEach(o =>
                    {
                        o.OutsourcId = req.outsourcId;
                        if (o.outsourcexpensespictures != null && o.outsourcexpensespictures.Count() > 0)
                        {
                            o.outsourcexpensespictures.ForEach(p =>
                            {
                                p.Id = Guid.NewGuid().ToString();
                                p.OutsourcExpensesId = o.Id;
                            });
                        }
                    });
                    await UnitWork.BatchAddAsync<OutsourcExpenses>(obj.OutsourcExpenses.ToArray());
                    await UnitWork.SaveAsync();
                    #endregion
                    if (!req.IsDraft)
                    {
                        if (string.IsNullOrWhiteSpace(outsourcObj.FlowInstanceId))
                        {
                            //创建结算流程
                            var mf = _moduleFlowSchemeApp.Get(m => m.Module.Name.Equals("个人代理结算"));
                            var afir = new AddFlowInstanceReq();
                            afir.SchemeId = mf.FlowSchemeId;
                            afir.FrmType = 2;
                            afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                            afir.CustomName = $"个人代理结算单";
                            afir.FrmData = "{\"ID\":\"" + outsourcObj.Id + "\"}";
                            afir.OrgId = loginContext.Orgs.OrderBy(o => o.CascadeId).FirstOrDefault()?.Id;
                            outsourcObj.FlowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(afir);
                            //增加全局待处理
                            var serviceOrederObj = await UnitWork.Find<ServiceOrder>(s => s.Id == outsourcObj.OutsourcExpenses.FirstOrDefault().ServiceOrderId).FirstOrDefaultAsync();
                            await _workbenchApp.AddOrUpdate(new WorkbenchPending
                            {
                                OrderType = 3,
                                TerminalCustomer = serviceOrederObj.TerminalCustomer,
                                TerminalCustomerId = serviceOrederObj.TerminalCustomerId,
                                ServiceOrderId = serviceOrederObj.Id,
                                ServiceOrderSapId = (int)serviceOrederObj.U_SAP_ID,
                                UpdateTime = DateTime.Now,
                                Remark = outsourcObj.Remark,
                                FlowInstanceId = obj.FlowInstanceId,
                                TotalMoney = obj.TotalMoney,
                                Petitioner = loginUser.Name,
                                SourceNumbers = outsourcObj.Id,
                                PetitionerId = loginUser.Id
                            });
                        }
                        else
                        {
                            await _flowInstanceApp.Start(new StartFlowInstanceReq { FlowInstanceId = outsourcObj.FlowInstanceId });
                        }

                    }

                    await UnitWork.UpdateAsync<Outsourc>(o => o.Id == req.outsourcId, u => new Outsourc
                    {
                        TotalMoney = obj.TotalMoney,
                        FlowInstanceId = outsourcObj.FlowInstanceId,
                        UpdateTime = DateTime.Now,
                        //todo:补充或调整自己需要的字段
                    });
                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("添加结算单失败。请重试" + ex.Message);
                }
            }
        }

        /// <summary>
        /// 审批
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Accraditation(AccraditationOutsourcReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var outsourcObj = await UnitWork.Find<Outsourc>(o => o.Id == int.Parse(req.OutsourcId)).Include(o => o.OutsourcExpenses).FirstOrDefaultAsync();
            VerificationReq VerificationReqModle = new VerificationReq
            {
                NodeRejectStep = "",
                NodeRejectType = "0",
                FlowInstanceId = outsourcObj.FlowInstanceId,
                VerificationFinally = "1",
                VerificationOpinion = req.Remark,
            };
            if (req.IsReject)
            {
                VerificationReqModle.VerificationFinally = "3";
                VerificationReqModle.VerificationOpinion = req.Remark;
                VerificationReqModle.NodeRejectType = "1";
                await _flowInstanceApp.Verification(VerificationReqModle);
                await UnitWork.UpdateAsync<OutsourcExpenses>(o => o.OutsourcId == outsourcObj.Id && o.ExpenseType == 3, o => new OutsourcExpenses { Money = 0 });
                await UnitWork.DeleteAsync<OutsourcExpenseOrg>(o => outsourcObj.OutsourcExpenses.Select(e => e.Id).Contains(o.ExpenseId));
                if (!string.IsNullOrWhiteSpace(outsourcObj.QuotationId.ToString())) await _quotationApp.CancellationSalesOrder(new QueryQuotationListReq { QuotationId = outsourcObj.QuotationId });
            }
            else
            {
                var flowInstanceObj = await UnitWork.Find<FlowInstance>(f => f.Id.Equals(outsourcObj.FlowInstanceId)).FirstOrDefaultAsync();
                if (flowInstanceObj.ActivityName.Equals("客服主管审批"))
                {
                    var expensesOrg = new OutsourcExpenses();
                    if (!string.IsNullOrWhiteSpace(req.Money))
                    {
                        if (outsourcObj.OutsourcExpenses.Where(o => o.ExpenseType == 3).Count() > 0)
                        {
                            await UnitWork.UpdateAsync<OutsourcExpenses>(o => o.ExpenseType == 3 && o.OutsourcId == outsourcObj.Id, o => new OutsourcExpenses { Money = decimal.Parse(req.Money), StartTime = DateTime.Now, EndTime = DateTime.Now });
                            expensesOrg = outsourcObj.OutsourcExpenses.Where(o => o.ExpenseType == 3).FirstOrDefault();
                        }
                        else
                        {
                            var outsourcexpensesObj = outsourcObj.OutsourcExpenses.FirstOrDefault();
                            expensesOrg = await UnitWork.AddAsync<OutsourcExpenses>(new OutsourcExpenses
                            {
                                ExpenseType = 3,
                                Money = decimal.Parse(req.Money),
                                ServiceOrderId = outsourcexpensesObj?.ServiceOrderId,
                                ServiceOrderSapId = outsourcexpensesObj.ServiceOrderSapId,
                                TerminalCustomer = outsourcexpensesObj.TerminalCustomer,
                                TerminalCustomerId = outsourcexpensesObj.TerminalCustomerId,
                                OutsourcId = outsourcObj.Id,
                                StartTime = DateTime.Now,
                                EndTime = DateTime.Now,
                            });
                        }
                        //outsourcObj.TotalMoney += decimal.Parse(req.Money);
                        outsourcObj.TotalMoney = outsourcObj.OutsourcExpenses.Sum(x => x.Money) + decimal.Parse(req.Money);
                    }
                    var outsourcExpenseOrgs = req.OutsourcExpenseOrgReqs.MapToList<OutsourcExpenseOrg>();
                    outsourcExpenseOrgs.ForEach(o =>
                    {
                        o.CreateTime = DateTime.Now;
                        o.UpdateTime = DateTime.Now;
                        o.ExpenseSatus = 1;
                        o.ExpenseId = string.IsNullOrWhiteSpace(o.ExpenseId) ? expensesOrg.Id : o.ExpenseId;
                        o.ExpenseType = string.IsNullOrWhiteSpace(o.ExpenseId) ? 3 : o.ExpenseType;
                        var moeny = string.IsNullOrWhiteSpace(o.ExpenseId) ? decimal.Parse(req.Money) : outsourcObj.OutsourcExpenses.Where(e => e.Id.Equals(o.ExpenseId)).FirstOrDefault()?.Money;
                        o.Money = moeny * (o.Ratio / 100);
                    });
                    await UnitWork.BatchAddAsync<OutsourcExpenseOrg>(outsourcExpenseOrgs.ToArray());
                    var Address = (await _quotationApp.CardAddress(outsourcObj.OutsourcExpenses.Select(o => o.TerminalCustomerId).ToList())).FirstOrDefault();
                    req.Money = string.IsNullOrWhiteSpace(req.Money) ? "0" : req.Money;
                    var quotationId = await _quotationApp.Add(new AddOrUpdateQuotationReq
                    {
                        IsOutsourc = true,
                        ServiceOrderId = (int)outsourcObj.OutsourcExpenses.FirstOrDefault()?.ServiceOrderId,
                        ServiceOrderSapId = (int)outsourcObj.OutsourcExpenses.FirstOrDefault()?.ServiceOrderSapId,
                        AcquisitionWay = "1",
                        AcceptancePeriod = 7,
                        DeliveryDate = DateTime.Now.AddDays(1),
                        CreateUser = outsourcObj.CreateUser,
                        CreateUserId = outsourcObj.CreateUserId,
                        InvoiceCompany = "",
                        CollectionAddress = Address.BillingAddress,
                        ShippingAddress = Address.DeliveryAddress,
                        IsDraft = false,
                        DeliveryMethod = "1",
                        ErpOrApp = 1,
                        IsMaterialType = "3",
                        MoneyMeans = "1",
                        ServiceChargeSMCost = decimal.Parse(req.Money),
                        ServiceChargeManHourSM = 1,
                        TravelExpenseCost = outsourcObj.OutsourcExpenses.Sum(o => o.Money),
                        TravelExpenseManHour = 1,
                        QuotationProducts = new List<QuotationProductReq>()
                    });
                    outsourcObj.QuotationId = int.Parse(quotationId);

                    //outSource和outSourceExpense为一对多关系,当服务方式为远程时,只会产生远程费用,实际是一对一的方式
                    if (outsourcObj.ServiceMode == 2)
                    {
                        //在客服主管审批这一步也做一次结算金额的调整,方便客服主管审批完之后查看
                        outsourcObj.TotalMoney = 0;
                        //判断该用户在当月已经有了多少单(除开本单以及被驳回的)
                        var completeDate = outsourcObj.OutsourcExpenses.FirstOrDefault(x => x.ExpenseType == 4)?.CompleteTime;
                        var startDate = new DateTime(completeDate.Value.Year, completeDate.Value.Month, 1);
                        var endDate = startDate.AddMonths(1);
                        var countInMonth = await (from o in UnitWork.Find<Outsourc>(null)
                                                  join oe in UnitWork.Find<OutsourcExpenses>(null)
                                                  on o.Id equals oe.OutsourcId
                                                  where o.CreateUserId == outsourcObj.CreateUserId
                                                  && oe.ExpenseType == 4 && oe.IsOverseas == false
                                                  && o.Id != outsourcObj.Id && o.IsRejected == false
                                                  && oe.CompleteTime >= startDate && oe.CompleteTime < endDate
                                                  //&& oe.SerialNumber != null && oe.SerialNumber > 0
                                                  select o.Id).Distinct().CountAsync();
                        outsourcObj.OutsourcExpenses.ForEach(o =>
                        {
                            o.SerialNumber = ++countInMonth;
                            if (o.IsOverseas)
                            {
                                o.SerialNumber = 0;
                                o.Money = 50;
                            }
                            else
                            {
                                o.Money = Calculation((int)o.SerialNumber);
                            }

                            outsourcObj.TotalMoney += o.Money;
                        });
                        await UnitWork.BatchUpdateAsync(outsourcObj.OutsourcExpenses.ToArray());
                    }
                }
                if (flowInstanceObj.ActivityName.Equals("总经理审批") && outsourcObj.ServiceMode == 2)
                {
                    var outsourcIds = await UnitWork.Find<Outsourc>(o => o.CreateUserId.Equals(outsourcObj.CreateUserId) && o.Id != outsourcObj.Id).Select(s => s.Id).ToListAsync();
                    outsourcObj.TotalMoney = 0;
                    var lastMonth =await UnitWork.Find<OutsourcExpenses>(e => outsourcIds.Contains((int)e.OutsourcId) && e.CompleteTime.Value.Year == outsourcObj.OutsourcExpenses.FirstOrDefault().CompleteTime.Value.Year && e.CompleteTime.Value.Month == outsourcObj.OutsourcExpenses.FirstOrDefault().CompleteTime.Value.Month && e.ExpenseType == 4 && e.SerialNumber != null && e.SerialNumber > 0 && e.IsOverseas == false).CountAsync();
                    outsourcObj.OutsourcExpenses.ForEach(o =>
                    {
                        o.SerialNumber = lastMonth + 1;
                        lastMonth++;
                        if (o.IsOverseas)
                        {
                            o.SerialNumber = 0;
                            o.Money = 50;
                        }
                        else
                        {
                            o.Money = Calculation((int)o.SerialNumber);
                        }

                        outsourcObj.TotalMoney += o.Money;
                    });
                    await UnitWork.BatchUpdateAsync<OutsourcExpenses>(outsourcObj.OutsourcExpenses.ToArray());
                }
                if (flowInstanceObj.ActivityName.Equals("财务支付"))
                {
                    outsourcObj.PayTime = DateTime.Now;
                    await _quotationApp.TimeOfDelivery((int)outsourcObj.QuotationId);
                }
                await _flowInstanceApp.Verification(VerificationReqModle);
            }
            await UnitWork.UpdateAsync<Outsourc>(r => r.Id == outsourcObj.Id, r => new Outsourc
            {
                //Status = returnNoteStatus,
                UpdateTime = DateTime.Now,
                TotalMoney = outsourcObj.TotalMoney,
                PayTime = outsourcObj.PayTime,
                QuotationId = outsourcObj.QuotationId
            });
            //修改全局待处理
            await UnitWork.UpdateAsync<WorkbenchPending>(w => w.SourceNumbers == outsourcObj.Id && w.OrderType == 3, w => new WorkbenchPending
            {
                TotalMoney = outsourcObj.TotalMoney,
                UpdateTime = DateTime.Now,
            });
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 批量审批
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task BatchAccraditation(List<AccraditationOutsourcReq> req)
        {
            if (req.Count() > 0)
            {
                foreach (var item in req)
                {
                    await Accraditation(item);
                }
            }
        }
        /// <summary>
        /// 驳回单个
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> ASingleRejection(AccraditationOutsourcReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var dbContext = UnitWork.GetDbContext<Outsourc>();
            var result = new TableData();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var outsourcObj = await UnitWork.Find<Outsourc>(o => o.Id == int.Parse(req.OutsourcId)).Include(o => o.OutsourcExpenses).FirstOrDefaultAsync();
                    var outsourcexpensesObj = outsourcObj.OutsourcExpenses.Where(o => o.Id.Equals(req.outsourcexpensesId)).FirstOrDefault();
                    await UnitWork.DeleteAsync<OutsourcExpenses>(outsourcexpensesObj);
                    await UnitWork.SaveAsync();
                    outsourcObj.OutsourcExpenses = outsourcObj.OutsourcExpenses.Where(o => o.Id != req.outsourcexpensesId).ToList();
                    var thisMonth = await UnitWork.Find<OutsourcExpenses>(e => e.CompleteTime.Value.Year == DateTime.Now.Year && e.CompleteTime.Value.Month == DateTime.Now.Month && e.ExpenseType == 4).CountAsync();
                    var lastMonth = await UnitWork.Find<OutsourcExpenses>(e => e.CompleteTime.Value.Year == DateTime.Now.Year && e.CompleteTime.Value.Month == DateTime.Now.Month - 1 && e.ExpenseType == 4).CountAsync();
                    var completionReportList = await UnitWork.Find<CompletionReport>(c => outsourcObj.OutsourcExpenses.Select(o => o.ServiceOrderId).ToList().Contains(c.ServiceOrderId)).ToListAsync();
                    outsourcObj.TotalMoney = 0;
                    outsourcObj.OutsourcExpenses.ForEach(o =>
                    {
                        o.CompleteTime = completionReportList.Where(c => c.ServiceOrderId == o.ServiceOrderId).OrderByDescending(c => c.CreateTime).FirstOrDefault()?.CreateTime;
                        if (o.CompleteTime.Value.Month == DateTime.Now.Month)
                        {
                            o.SerialNumber = thisMonth + 1;
                            thisMonth++;
                        }
                        else if (o.CompleteTime.Value.Month == DateTime.Now.Month - 1)
                        {
                            o.SerialNumber = lastMonth + 1;
                            lastMonth++;
                        }
                        o.Money = Calculation((int)o.SerialNumber);
                        outsourcObj.TotalMoney += o.Money;
                    });
                    await UnitWork.BatchUpdateAsync<OutsourcExpenses>(outsourcObj.OutsourcExpenses.ToArray());

                    await UnitWork.UpdateAsync<Outsourc>(r => r.Id == outsourcObj.Id, r => new Outsourc
                    {

                        //Status = returnNoteStatus,
                        UpdateTime = DateTime.Now,
                        TotalMoney = outsourcObj.TotalMoney,
                        IsRejected = true
                    });

                    await UnitWork.UpdateAsync<CompletionReport>(c => c.CreateUserId.Equals(outsourcObj.CreateUserId) && c.ServiceOrderId == outsourcexpensesObj.ServiceOrderId, c => new CompletionReport { IsReimburse = 1 });
                    #region 增加驳回记录
                    FlowInstanceOperationHistory flowInstanceOperationHistory = new FlowInstanceOperationHistory
                    {
                        InstanceId = outsourcObj.FlowInstanceId,
                        CreateUserId = loginContext.User.Id,
                        CreateUserName = loginContext.User.Name,
                        CreateDate = DateTime.Now,
                        Content = "驳回服务单" + outsourcexpensesObj.ServiceOrderSapId,
                        Remark = req.Remark,
                        ApprovalResult = "驳回",
                    };
                    var fioh = await UnitWork.Find<FlowInstanceOperationHistory>(r => r.InstanceId.Equals(outsourcObj.FlowInstanceId)).OrderByDescending(r => r.CreateDate).FirstOrDefaultAsync();
                    if (fioh != null)
                    {
                        flowInstanceOperationHistory.IntervalTime = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(fioh.CreateDate)).TotalSeconds);
                    }
                    await UnitWork.AddAsync(flowInstanceOperationHistory);
                    #endregion
                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                    result.Data = new { TotalMoney = outsourcObj.TotalMoney };
                    return result;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("驳回失败。请重试" + ex.Message);
                }
            }


        }

        /// <summary>
        /// 撤回单个
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task SingleRecall(AccraditationOutsourcReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //如果用户的角色不包括客服主管,则不能进行撤回操作
            if (!loginContext.Roles.Any(r => r.Name.Equals("客服主管")))
            {
                throw new Exception("只有客服主管能进行撤回操作");
            }

            var outsourcObj = await UnitWork.Find<Outsourc>(o => o.Id == int.Parse(req.OutsourcId)).Include(o => o.OutsourcExpenses).FirstOrDefaultAsync();
            var flowInstanceObj = await UnitWork.Find<FlowInstance>(f => f.Id.Equals(outsourcObj.FlowInstanceId)).FirstOrDefaultAsync();
            //总经理审批环节完了之后，到了下一步，客服主管不能再撤回
            if (flowInstanceObj.ActivityName.Equals("财务支付"))
            {
                throw new Exception("当前流程状态不能撤回");
            }

            RecallFlowInstanceReq recallFlowInstanceReq = new RecallFlowInstanceReq
            {
                NodeRejectType = "0", //0代表返回上一节点
                NodeRejectStep = "", //当NodeRejectType = 2时,需要指明节点
                FlowInstanceId = outsourcObj.FlowInstanceId,
            };

            using var tran = await UnitWork.GetDbContext<Outsourc>().Database.BeginTransactionAsync();
            try
            {
                //撤回操作
                await _flowInstanceApp.ReCall2(recallFlowInstanceReq);
                //调整金额
                await UnitWork.UpdateAsync<OutsourcExpenses>(o => o.OutsourcId == outsourcObj.Id && o.ExpenseType == 3, o => new OutsourcExpenses { Money = 0 });
                await UnitWork.DeleteAsync<OutsourcExpenseOrg>(o => outsourcObj.OutsourcExpenses.Select(e => e.Id).Contains(o.ExpenseId));
                if (!string.IsNullOrWhiteSpace(outsourcObj.QuotationId.ToString())) await _quotationApp.CancellationSalesOrder(new QueryQuotationListReq { QuotationId = outsourcObj.QuotationId });
                //将工时费用重置为0,其他费用不变
                outsourcObj.OutsourcExpenses.Where(x => x.ExpenseType == 3).All(x => { x.Money = 0; return true; });
                //重新计算总费用
                outsourcObj.TotalMoney = outsourcObj.OutsourcExpenses.Sum(x => x.Money);

                await UnitWork.UpdateAsync<Outsourc>(r => r.Id == outsourcObj.Id, r => new Outsourc
                {
                    UpdateTime = DateTime.Now,
                    TotalMoney = outsourcObj.TotalMoney,
                    PayTime = outsourcObj.PayTime,
                    QuotationId = outsourcObj.QuotationId
                });
                //修改全局待处理
                await UnitWork.UpdateAsync<WorkbenchPending>(w => w.SourceNumbers == outsourcObj.Id && w.OrderType == 3, w => new WorkbenchPending
                {
                    TotalMoney = outsourcObj.TotalMoney,
                    UpdateTime = DateTime.Now,
                });
                await UnitWork.SaveAsync();
                await tran.CommitAsync();
            }
            catch(Exception ex)
            {
                await tran.RollbackAsync();
                throw new Exception("撤回失败。请重试" + ex.Message);
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Delete(QueryoutsourcListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var outsourcObj = await UnitWork.Find<Outsourc>(o => o.Id == int.Parse(req.OutsourcId)).Include(o => o.OutsourcExpenses).ThenInclude(o => o.outsourcexpensespictures).FirstOrDefaultAsync();

            if (outsourcObj != null)
            {
                var status = true;
                if (!string.IsNullOrWhiteSpace(outsourcObj.FlowInstanceId))
                {
                    status = (await UnitWork.Find<FlowInstance>(f => f.Id.Equals(outsourcObj.FlowInstanceId)).FirstOrDefaultAsync())?.ActivityName == "开始" ? true : false;
                    await UnitWork.DeleteAsync<FlowInstanceTransitionHistory>(f => f.InstanceId.Equals(outsourcObj.FlowInstanceId));
                    await UnitWork.DeleteAsync<FlowInstanceOperationHistory>(f => f.InstanceId.Equals(outsourcObj.FlowInstanceId));
                    await UnitWork.DeleteAsync<FlowInstance>(f => f.Id.Equals(outsourcObj.FlowInstanceId));
                }
                if (status)
                {
                    await UnitWork.DeleteAsync<Outsourc>(o => o.Id == int.Parse(req.OutsourcId));
                    var serviceOrderids = outsourcObj.OutsourcExpenses.Select(o => o.ServiceOrderId).Distinct().ToList();
                    await UnitWork.UpdateAsync<CompletionReport>(c => c.CreateUserId.Equals(outsourcObj.CreateUserId) && serviceOrderids.Contains(c.ServiceOrderId), c => new CompletionReport { IsReimburse = 1 });
                    await UnitWork.SaveAsync();
                }
                else
                {
                    throw new Exception("当前状态不可删除。");
                }

            }
            else
            {
                throw new Exception("当前状态不可删除。");
            }

        }

        /// <summary>
        /// 删除单个费用
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task DeleteExpenses(QueryoutsourcListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var outsourcexpensesObj = await UnitWork.Find<OutsourcExpenses>(o => o.Id.Equals(req.OutsourcExpensesId)).FirstOrDefaultAsync();

            if (outsourcexpensesObj != null)
            {
                await UnitWork.DeleteAsync<OutsourcExpenses>(o => o.Id.Equals(req.OutsourcExpensesId));
                await UnitWork.UpdateAsync<Outsourc>(o => o.Id == outsourcexpensesObj.OutsourcId, o => new Outsourc { TotalMoney = o.TotalMoney - outsourcexpensesObj.Money });
                await UnitWork.SaveAsync();
            }

        }


        /// <summary>
        /// 新增修改通用接口
        /// </summary>
        /// <returns name="req"></returns>
        public async Task<Outsourc> Condition(AddOrUpdateoutsourcReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var obj = req.MapTo<Outsourc>();
            obj.UpdateTime = DateTime.Now;
            obj.IsRejected = false;
            var completionReportList = await UnitWork.Find<CompletionReport>(c => (obj.ServiceMode == 1 && c.ServiceOrderId == req.ServiceOrderId) || (obj.ServiceMode == 2 && obj.OutsourcExpenses.Select(o => o.ServiceOrderId).ToList().Contains(c.ServiceOrderId))).ToListAsync();
            obj.TotalMoney = 0;
            if (obj.ServiceMode == 1)
            {
                var completionReportObj = completionReportList.Where(c => c.ServiceOrderId == req.ServiceOrderId).OrderByDescending(c => c.CreateTime).FirstOrDefault();

                if (obj.OutsourcExpenses == null || obj.OutsourcExpenses.Count() <= 0)
                {
                    obj.OutsourcExpenses.Add(new OutsourcExpenses
                    {
                        ExpenseType = 3,
                        Money = 0,
                        ServiceOrderId = completionReportObj?.ServiceOrderId,
                        ServiceOrderSapId = req.ServiceOrderSapId,
                        TerminalCustomer = completionReportObj.TerminalCustomer,
                        TerminalCustomerId = completionReportObj.TerminalCustomerId,
                    });
                }
                obj.OutsourcExpenses.ForEach(o =>
                {
                    obj.TotalMoney += o.Money;
                    o.CompleteTime = completionReportObj?.CreateTime;
                    o.TerminalCustomer = completionReportObj.TerminalCustomer;
                    o.TerminalCustomerId = completionReportObj.TerminalCustomerId;
                });
            }
            else
            {
                var serviceOrderIds = await UnitWork.Find<ServiceOrder>(s => obj.OutsourcExpenses.Select(o => o.ServiceOrderId).ToList().Contains(s.Id)).ToListAsync();
                var outsourcIds = await UnitWork.Find<Outsourc>(o => o.CreateUserId.Equals(loginUser.Id)).WhereIf(!string.IsNullOrWhiteSpace(req.outsourcId.ToString()), e => e.Id != req.outsourcId).Select(s => s.Id).ToListAsync();
                var thisMonth = await UnitWork.Find<OutsourcExpenses>(e => outsourcIds.Contains((int)e.OutsourcId) && e.CompleteTime.Value.Year == DateTime.Now.Year && e.CompleteTime.Value.Month == DateTime.Now.Month && e.ExpenseType == 4 && e.IsOverseas == false).CountAsync();
                var lastMonth = await UnitWork.Find<OutsourcExpenses>(e => outsourcIds.Contains((int)e.OutsourcId) && e.CompleteTime.Value.Year == DateTime.Now.Year && e.CompleteTime.Value.Month == DateTime.Now.Month - 1 && e.ExpenseType == 4 && e.IsOverseas == false).CountAsync();
                var globalarea = await UnitWork.Find<GlobalArea>(g => g.AreaLevel == "3" && g.Pid == "99").Select(g => g.AreaName).ToListAsync();
                var number = 0;
                obj.OutsourcExpenses.ForEach(o =>
                {
                    var completionReportObj = completionReportList.Where(c => c.ServiceOrderId == o.ServiceOrderId).OrderByDescending(c => c.CreateTime).FirstOrDefault();
                    o.ExpenseType = 4;
                    o.CompleteTime = completionReportObj?.CreateTime;
                    o.TerminalCustomer = completionReportObj.TerminalCustomer;
                    o.TerminalCustomerId = completionReportObj.TerminalCustomerId;
                    if (o.CompleteTime.Value.Month == DateTime.Now.Month)
                    {
                        number = thisMonth + 1;
                    }
                    else if (o.CompleteTime.Value.Month == DateTime.Now.Month - 1)
                    {
                        number = lastMonth + 1;
                    }
                    var Province = serviceOrderIds.Where(s => s.Id == o.ServiceOrderId).FirstOrDefault()?.Province;
                    if (globalarea.Contains(Province) || Province == "海外")
                    {
                        o.Money = 50;
                        o.IsOverseas = true;
                    }
                    else
                    {
                        o.Money = Calculation(number);
                        o.IsOverseas = false;
                    }
                    obj.TotalMoney += o.Money;
                });
            }
            return obj;
        }

        /// <summary>
        /// 计算金额
        /// </summary>
        /// <param name="SerialNumber"></param>
        /// <returns></returns>
        public int Calculation(int SerialNumber)
        {
            var Money = 0;
            if (SerialNumber > 300)
            {
                Money = 60;
            }
            else if (SerialNumber > 250)
            {
                Money = 50;
            }
            else if (SerialNumber > 200)
            {
                Money = 45;
            }
            else if (SerialNumber > 150)
            {
                Money = 40;
            }
            else if (SerialNumber > 100)
            {
                Money = 35;
            }
            else
            {
                Money = 30;
            }
            return Money;
        }

        /// <summary>
        /// 解析完工报告json
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<string> GetServiceTroubleAndSolution(string data, string objectCode)
        {
            List<string> result = new List<string>();
            if (!string.IsNullOrEmpty(data))
            {
                JArray jArray = (JArray)JsonConvert.DeserializeObject(data);
                foreach (var item in jArray)
                {
                    result.Add(item[objectCode] == null ? "" : item[objectCode].ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<byte[]> ExportExcel(QueryoutsourcListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //List<int?> outsourcIds = new List<int?>();
            List<int?> serviceOrderId = new List<int?>();
            if (!string.IsNullOrWhiteSpace(request.CompletionStartTime.ToString()) || !string.IsNullOrWhiteSpace(request.CompletionEndTime.ToString()))
            {
                var completion = await UnitWork.Find<CompletionReport>(c => c.IsReimburse == 4)
                    .WhereIf(!string.IsNullOrWhiteSpace(request.CompletionStartTime.ToString()), c => c.EndDate > request.CompletionStartTime)
                    .WhereIf(!string.IsNullOrWhiteSpace(request.CompletionEndTime.ToString()), c => c.EndDate < Convert.ToDateTime(request.CompletionEndTime).AddDays(1))
                    .Select(c => c.ServiceOrderId)
                    .ToListAsync();
                serviceOrderId.AddRange(completion);
            }

            var outsourcIds = await UnitWork.Find<OutsourcExpenses>(null)
                .WhereIf(serviceOrderId.Count > 0, o => serviceOrderId.Contains(o.ServiceOrderId))
                .WhereIf(!string.IsNullOrWhiteSpace(request.ServiceOrderSapId), o => o.ServiceOrderSapId == int.Parse(request.ServiceOrderSapId))
                .WhereIf(!string.IsNullOrWhiteSpace(request.Customer), o => o.TerminalCustomer.Contains(request.Customer) || o.TerminalCustomerId.Contains(request.Customer))
                .Select(c => c.OutsourcId)
                .Distinct()
                .ToListAsync();

            var result = new TableData();
            var query = UnitWork.Find<Outsourc>(null).Include(c => c.OutsourcExpenses)
                        .WhereIf(!string.IsNullOrWhiteSpace(request.CreateName), q => q.CreateUser.Contains(request.CreateName))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.OutsourcId), q => q.Id == int.Parse(request.OutsourcId))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.StartTime.ToString()), q => q.CreateTime > request.StartTime)
                       .WhereIf(!string.IsNullOrWhiteSpace(request.EndTime.ToString()), q => q.CreateTime < Convert.ToDateTime(request.EndTime).AddDays(1))
                       .Where(o => outsourcIds.Contains(o.Id));

            if (loginContext.User.Account != Define.SYSTEM_USERNAME)
            {
                #region 筛选条件
                //var schemeContent = await .FirstOrDefaultAsync();
                List<string> Lines = new List<string>();
                List<string> flowInstanceIds = new List<string>();
                var lineId = "";
                var SchemeContent = await UnitWork.Find<FlowScheme>(f => f.SchemeName.Equals("个人代理结算")).Select(f => f.SchemeContent).FirstOrDefaultAsync();
                SchemeContentJson schemeJson = JsonHelper.Instance.Deserialize<SchemeContentJson>(SchemeContent);
                if (request.PageType != null && request.PageType > 0)
                {
                    if (loginContext.Roles.Any(r => r.Name.Equals("客服主管")))
                    {
                        lineId = schemeJson.Nodes.Where(n => n.name.Equals("客服主管审批")).FirstOrDefault()?.id;
                    }
                    else if (loginContext.Roles.Any(r => r.Name.Equals("总经理")))
                    {
                        lineId = schemeJson.Nodes.Where(n => n.name.Equals("总经理审批")).FirstOrDefault()?.id;
                    }
                    else if (loginContext.Roles.Any(r => r.Name.Equals("出纳")))
                    {
                        lineId = schemeJson.Nodes.Where(n => n.name.Equals("财务支付")).FirstOrDefault()?.id;
                    }
                }
                switch (request.PageType)
                {
                    case 1:

                        Lines.Add(lineId);
                        break;
                    case 2:
                        List<string> lineIds = new List<string>();
                        var lineIdTo = lineId;
                        foreach (var item in schemeJson.Lines)
                        {
                            if (schemeJson.Lines.Where(l => l.from.Equals(lineIdTo)).FirstOrDefault()?.to != null)
                            {
                                lineIdTo = schemeJson.Lines.Where(l => l.from.Equals(lineIdTo)).FirstOrDefault()?.to;
                                lineIds.Add(lineIdTo);
                            }
                            else
                            {
                                break;
                            }
                        }
                        Lines.AddRange(lineIds);
                        break;
                    case 3:
                        Lines.Add(lineId);
                        break;
                    case 4:
                        if (loginContext.Roles.Any(r => r.Name.Equals("出纳")))
                        {
                            Lines.Add(schemeJson.Nodes.Where(n => n.name.Equals("结束")).FirstOrDefault()?.id);
                        }
                        break;
                    default:
                        var orgRole = await UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.Key == Define.ORGROLE && c.FirstId == loginContext.User.Id).FirstOrDefaultAsync();
                        if (orgRole != null)//查看本部下数据
                        {
                            var orgId = orgRole.SecondId;
                            var userId = await UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.SecondId == orgId && c.Key == Define.USERORG).Select(c => c.FirstId).ToListAsync();
                            query = query.Where(r => userId.Contains(r.CreateUserId));
                        }
                        else
                        {
                            query = query.Where(q => q.CreateUserId.Equals(loginContext.User.Id));
                        }
                        break;
                }
                if (Lines.Count > 0)
                {
                    flowInstanceIds = await UnitWork.Find<FlowInstance>(f => Lines.Contains(f.ActivityId)).Select(s => s.Id).ToListAsync();
                    query = query.Where(q => flowInstanceIds.Contains(q.FlowInstanceId));
                }

                #endregion
            }

            var outsourcList = await query.OrderByDescending(o => o.UpdateTime).ToListAsync();
            var userIds = outsourcList.Select(o => o.CreateUserId).ToList();
            var SelOrgName = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(null).Select(o => new { o.Id, o.Name, o.CascadeId }).ToListAsync();
            var Relevances = await UnitWork.Find<Relevance>(r => r.Key == Define.USERORG && userIds.Contains(r.FirstId)).Select(r => new { r.FirstId, r.SecondId }).ToListAsync();

            //modify by yangis @2022.03.14 导出内容中新加售后归属字段
            var customerIds = new List<string>(); //客户代码
            outsourcList.ForEach(x => customerIds.AddRange(x.OutsourcExpenses.Select(o => o.TerminalCustomerId)));
            //根据客户代码查找售后主管
            var supervisors = await (from r in UnitWork.Find<OCRD>(null)
                                     join e in UnitWork.Find<OHEM>(null) on r.DfTcnician equals e.empID
                                     where customerIds.Distinct().Contains(r.CardCode)
                                     select new
                                     {
                                         customerId = r.CardCode ?? "",
                                         supervisorName = (e.lastName ?? "") + (e.firstName ?? "")
                                     }).ToListAsync();
            //根据姓名查找部门
            Func<IEnumerable<string>, Task<List<UserResp>>> getOrgName = async x =>
             await (from u in UnitWork.Find<User>(null)
                    join r in UnitWork.Find<Relevance>(null) on u.Id equals r.FirstId
                    join o in UnitWork.Find<Repository.Domain.Org>(null) on r.SecondId equals o.Id
                    where x.Contains(u.Name) && r.Key == Define.USERORG
                    orderby o.CascadeId descending
                    select new UserResp { Name = u.Name ?? "", OrgName = o.Name ?? "", CascadeId = o.CascadeId ?? "" }).ToListAsync();
            var orgNames = await getOrgName(supervisors.Select(s => s.supervisorName).Distinct());
            //将客户代码、售后主管、部门进行数据合并
            var supervisorData = from s in supervisors
                                 join o in orgNames on s.supervisorName equals o.Name
                                 select new
                                 {
                                     CustomerId = s.customerId ?? "",
                                     SupervisorName = (o.OrgName ?? "") + (s.supervisorName ?? ""),
                                     OrgName = o.OrgName ?? "",
                                     CascadeId = o.CascadeId ?? ""
                                 };

            List<OutsourcExcelDto> outsourcs = new List<OutsourcExcelDto>();
            outsourcList.ForEach(o =>
            {
                var orgName = SelOrgName.Where(s => s.Id.Equals(Relevances.Where(r => r.FirstId.Equals(o.CreateUserId)).FirstOrDefault()?.SecondId)).FirstOrDefault()?.Name;
                var outsourcexpensesObj = o.OutsourcExpenses.FirstOrDefault();
                outsourcs.Add(new OutsourcExcelDto
                {
                    Id = o.Id,
                    ServiceOrderSapId = outsourcexpensesObj?.ServiceOrderSapId,
                    CustomerId = outsourcexpensesObj?.TerminalCustomerId,
                    CustomerName = outsourcexpensesObj?.TerminalCustomer,
                    Supervisor = supervisorData.Where(s => s.CustomerId == outsourcexpensesObj?.TerminalCustomerId).OrderByDescending(x => x.CascadeId)?.Take(1)?.FirstOrDefault()?.SupervisorName,
                    ServiceFee = o.OutsourcExpenses.Where(c => c.ExpenseType == 4).Sum(c => c.Money),
                    WorkingHoursFee = o.OutsourcExpenses.Where(c => c.ExpenseType == 3).Sum(c => c.Money),
                    TransportationFee = o.OutsourcExpenses.Where(c => c.ExpenseType == 1).Sum(c => c.Money),
                    AccommodationFee = o.OutsourcExpenses.Where(c => c.ExpenseType == 2).Sum(c => c.Money),
                    TotalMoney = o.TotalMoney,
                    CreateUser = orgName == null ? o.CreateUser : orgName + "-" + o.CreateUser
                });
            });
            IExporter exporter = new ExcelExporter();
            var bytes = await exporter.ExportAsByteArray(outsourcs);
            return bytes;
        }

        #region MyRegion
        //public async Task Test()
        //{
        //    var data = await UnitWork.Find<FlowInstance>(c => c.MakerList == "2093e7b3-c7c6-11ea-bc9e-54bf645e326d,204d6d30-c7c6-11ea-bc9e-54bf645e326d").ToListAsync();
        //    var flowid = data.Select(c => c.Id).ToList();

        //    List<FlowInstanceOperationHistory> listhis = new List<FlowInstanceOperationHistory>();
        //    data.ForEach(c =>
        //    {
        //        FlowInstanceOperationHistory flowInstanceOperationHistory = new FlowInstanceOperationHistory
        //        {
        //            InstanceId = c.Id,
        //            CreateUserId = "系统管理员",
        //            CreateUserName = "系统",
        //            CreateDate = DateTime.Now,
        //            ActivityId = c.ActivityId,
        //            Content = "系统",
        //            Remark = "人工已统计支付"
        //        }; //操作记录
        //        listhis.Add(flowInstanceOperationHistory);
        //    });

        //    await UnitWork.BatchAddAsync(listhis.ToArray());
        //    await UnitWork.SaveAsync();
        //}
        #endregion

        public OutsourcApp(IUnitWork unitWork, FlowInstanceApp flowInstanceApp, PendingApp pendingApp, WorkbenchApp workbenchApp,
            QuotationApp quotationApp, ModuleFlowSchemeApp moduleFlowSchemeApp, IAuth auth, UserManagerApp userManagerApp) : base(unitWork, auth)
        {
            _flowInstanceApp = flowInstanceApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
            _workbenchApp = workbenchApp;
            _quotationApp = quotationApp;
            _pendingApp = pendingApp;
            _userManagerApp = userManagerApp;
        }
    }
}