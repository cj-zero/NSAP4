using DinkToPdf;
using DotNetCore.CAP;
using Infrastructure;
using Infrastructure.Const;
using Infrastructure.Excel;
using Infrastructure.Export;
using Infrastructure.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npoi.Mapper;
using OpenAuth.App.Interface;
using OpenAuth.App.Material.Dto;
using OpenAuth.App.Material.Request;
using OpenAuth.App.Material.Response;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Sap.Request;
using OpenAuth.App.Workbench;
using OpenAuth.Repository;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Material;
using OpenAuth.Repository.Domain.NsapBone;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.Repository.Domain.Workbench;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAuth.App.Material
{
    /// <summary>
    /// 报价单操作
    /// </summary>
    public class QuotationApp : OnlyUnitWorkBaeApp
    {
        private readonly FlowInstanceApp _flowInstanceApp;
        private readonly IOptions<AppSetting> _appConfiguration;
        private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;
        public readonly WorkbenchApp _workbenchApp;
        static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);//用信号量代替锁
        private readonly OrgManagerApp _orgApp;
        private readonly UserManagerApp _userManagerApp;

        private ICapPublisher _capBus;

        /// <summary>
        /// 加载列表 ERP4.0 APP共用
        /// </summary>
        public async Task<TableData> Load(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var loginUserRole = loginContext.Roles;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(request.AppId));
                loginUserRole = await GetRoleByUserId(loginUser.Id);
            }
            var result = new TableData();
            List<int> ServiceOrderids = new List<int>();
            if (!string.IsNullOrWhiteSpace(request.CardCode))
            {
                ServiceOrderids = await UnitWork.Find<ServiceOrder>(null).Where(q => q.TerminalCustomer.Contains(request.CardCode) || q.TerminalCustomerId.Contains(request.CardCode)).Select(s => s.Id).ToListAsync();

            }
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            var Quotations = UnitWork.Find<Quotation>(null).Include(q => q.QuotationPictures).Include(q => q.QuotationOperationHistorys).WhereIf(request.QuotationId.ToString() != null, q => q.Id.ToString().Contains(request.QuotationId.ToString()))
                                .WhereIf(request.ServiceOrderSapId != null, q => q.ServiceOrderSapId.ToString().Contains(request.ServiceOrderSapId.ToString()))
                                .WhereIf(!string.IsNullOrWhiteSpace(request.CreateUser), q => q.CreateUser.Contains(request.CreateUser))
                                .WhereIf(request.StartCreateTime != null, q => q.CreateTime > request.StartCreateTime)
                                .WhereIf(request.EndCreateTime != null, q => q.CreateTime < request.EndCreateTime)
                                .WhereIf(request.Status != null, q => q.Status == request.Status)
                                .WhereIf(request.QuotationStatus != null, q => q.QuotationStatus == request.QuotationStatus)
                                .WhereIf(request.SalesOrderId != null, q => q.SalesOrderId == request.SalesOrderId)
                                .WhereIf(ServiceOrderids.Count() > 0, q => ServiceOrderids.Contains(q.ServiceOrderId))
                                .WhereIf(request.Remark != null, q => q.Remark.Contains(request.Remark))
                                .WhereIf(!string.IsNullOrWhiteSpace(request.CancelRequest), q => q.CancelRequest == int.Parse(request.CancelRequest))
                                .WhereIf(!string.IsNullOrWhiteSpace(request.MaterialCode), q => q.QuotationMergeMaterials.Any(x => x.MaterialCode == request.MaterialCode))
                                .WhereIf(request.IsFinlish == 1, q => q.DeliveryMethod == "1" && q.Status == 2)//首页报表已完成条件
                                .WhereIf(request.IsFinlish == 2, q => q.DeliveryMethod != "1")//首页报表 未完成条件
                                .Select(q => new
                                {
                                    q.Id,
                                    q.ServiceOrderSapId,
                                    q.ServiceOrderId,
                                    q.TotalMoney,
                                    q.TotalCommission,
                                    q.FlowInstanceId,
                                    q.UpDateTime,
                                    q.CreateUserId,
                                    q.CreateUser,
                                    q.Remark,
                                    q.SalesOrderId,
                                    q.CreateTime,
                                    q.QuotationStatus,
                                    q.IsDraft,
                                    q.Tentative,
                                    q.IsProtected,
                                    q.PrintWarehouse,
                                    q.CancelRequest,
                                    q.IsMaterialType,
                                    q.QuotationOperationHistorys,
                                    q.QuotationPictures
                                });
            //sw.Stop();
            //TimeSpan dt = sw.Elapsed;
            //double s = dt.TotalSeconds;

            var flowInstanceIds = await Quotations.Select(q => q.FlowInstanceId).ToListAsync();
            var flowinstanceObjs = from a in UnitWork.Find<FlowInstance>(f => flowInstanceIds.Contains(f.Id))
                                   join b in UnitWork.Find<FlowInstanceOperationHistory>(null) on a.Id equals b.InstanceId into ab
                                   from b in ab.DefaultIfEmpty()
                                   select new { a.Id, a.ActivityName, b.CreateUserId, b.Content, a.IsFinish };
            if (!loginUserRole.Any(r => r.Name.Equals("呼叫中心-查看")) && !loginUserRole.Any(r => r.Name.Equals("客服主管")) && !loginUser.Account.Equals(Define.SYSTEM_USERNAME))
            {
                var flowinstanceList = await flowinstanceObjs.ToListAsync();
                if (request.PageStart != null && request.PageStart == 1)
                {
                    if (loginUserRole.Any(r => r.Name.Equals("销售员")))
                    {
                        switch (request.StartType)
                        {
                            case 1:
                                flowInstanceIds = flowinstanceList.Where(f => f.ActivityName == "销售员审批").Select(f => f.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) || (string.IsNullOrWhiteSpace(q.FlowInstanceId) && q.QuotationStatus == 3.1M));
                                break;
                            case 2:
                                flowInstanceIds = flowinstanceList.Where(f => f.CreateUserId.Equals(loginUser.Id) && f.Content == "销售员审批").Select(f => f.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) || (string.IsNullOrWhiteSpace(q.FlowInstanceId) && q.QuotationStatus > 3.1M));
                                break;
                            default:
                                flowInstanceIds = flowinstanceList.Where(f => (f.CreateUserId.Equals(loginUser.Id) || f.ActivityName == "销售员审批")).Select(f => f.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) || (string.IsNullOrWhiteSpace(q.FlowInstanceId) && q.QuotationStatus >= 3.1M));
                                break;
                        }
                        ServiceOrderids = await UnitWork.Find<ServiceOrder>(null).Where(q => q.SalesManId.Equals(loginUser.Id)).WhereIf(ServiceOrderids.Count() > 0, s => ServiceOrderids.Contains(s.Id)).Select(s => s.Id).ToListAsync();
                        Quotations = Quotations.Where(q => ServiceOrderids.Contains(q.ServiceOrderId));
                    }
                    else if (loginUserRole.Any(r => r.Name.Equals("物料工程审批")))
                    {
                        switch (request.StartType)
                        {
                            case 1:
                                flowInstanceIds = flowinstanceList.Where(f => f.ActivityName == "工程审批").Select(f => f.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) || (string.IsNullOrWhiteSpace(q.FlowInstanceId) && q.QuotationStatus == 4));
                                break;
                            case 2:
                                flowInstanceIds = flowinstanceList.Where(f => f.CreateUserId.Equals(loginUser.Id) && f.Content == "工程审批").Select(f => f.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) || (string.IsNullOrWhiteSpace(q.FlowInstanceId) && q.QuotationStatus > 4));
                                break;
                            default:
                                flowInstanceIds = flowinstanceList.Where(f => (f.CreateUserId.Equals(loginUser.Id) || f.ActivityName == "工程审批")).Select(f => f.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) || (string.IsNullOrWhiteSpace(q.FlowInstanceId) && q.QuotationStatus >= 4));
                                break;
                        }
                    }
                    else if (loginUserRole.Any(r => r.Name.Equals("总经理")))
                    {
                        ServiceOrderids = await UnitWork.Find<ServiceOrder>(null).Where(q => q.SalesManId.Equals(loginUser.Id)).Select(s => s.Id).ToListAsync();
                        List<string> slpIds = null;
                        switch (request.StartType)
                        {
                            case 1:
                                flowInstanceIds = flowinstanceList.Where(f => f.ActivityName == "总经理审批").Select(f => f.Id).Distinct().ToList();
                                slpIds = flowinstanceList.Where(f => f.ActivityName == "销售员审批").Select(f => f.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) || (ServiceOrderids.Contains(q.ServiceOrderId) && slpIds.Contains(q.FlowInstanceId)) || (string.IsNullOrWhiteSpace(q.FlowInstanceId) && q.QuotationStatus == 5 || (ServiceOrderids.Contains(q.ServiceOrderId) && q.QuotationStatus == 3.1M)));
                                break;
                            case 2:
                                flowInstanceIds = flowinstanceList.Where(f => f.CreateUserId.Equals(loginUser.Id) && f.Content == "总经理审批").Select(f => f.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) || (string.IsNullOrWhiteSpace(q.FlowInstanceId) && q.QuotationStatus > 5 || (ServiceOrderids.Contains(q.ServiceOrderId) && q.QuotationStatus > 3.1M)));
                                break;
                            default:
                                flowInstanceIds = flowinstanceList.Where(f => (f.CreateUserId.Equals(loginUser.Id) || f.ActivityName == "总经理审批")).Select(f => f.Id).Distinct().ToList();
                                slpIds = flowinstanceList.Where(f => f.ActivityName == "销售员审批").Select(f => f.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) || (ServiceOrderids.Contains(q.ServiceOrderId) && slpIds.Contains(q.FlowInstanceId)) || (string.IsNullOrWhiteSpace(q.FlowInstanceId) && q.QuotationStatus >= 5 || (ServiceOrderids.Contains(q.ServiceOrderId) && q.QuotationStatus >= 3.1M)));
                                break;
                        }
                    }
                    else if (loginUserRole.Any(r => r.Name.Equals("销售总助")))
                    {
                        switch (request.StartType)
                        {
                            case 1:
                                flowInstanceIds = flowinstanceList.Where(f => f.ActivityName == "销售总助审批").Select(f => f.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId));
                                break;
                            case 2:
                                flowInstanceIds = flowinstanceList.Where(f => f.CreateUserId.Equals(loginUser.Id) && f.Content == "销售总助审批").Select(f => f.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId));
                                break;
                            default:
                                flowInstanceIds = flowinstanceList.Where(f => (f.CreateUserId.Equals(loginUser.Id) || f.ActivityName == "销售总助审批")).Select(f => f.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId));
                                break;
                        }
                    }
                    else
                    {
                        switch (request.StartType)
                        {
                            case 1:
                                flowInstanceIds = flowinstanceList.Where(f => f.ActivityName == "确认报价单").Select(f => f.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => (flowInstanceIds.Contains(q.FlowInstanceId) || (string.IsNullOrWhiteSpace(q.FlowInstanceId) && q.QuotationStatus == 6)) && q.CreateUserId.Equals(loginUser.Id));
                                break;

                            case 2:
                                flowInstanceIds = flowinstanceList.Where(f => f.CreateUserId.Equals(loginUser.Id) && f.Content == "确认报价单").Select(f => f.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => (flowInstanceIds.Contains(q.FlowInstanceId) || (string.IsNullOrWhiteSpace(q.FlowInstanceId) && q.QuotationStatus > 6)) && q.CreateUserId.Equals(loginUser.Id));
                                break;
                            default:
                                flowInstanceIds = flowinstanceList.Where(f => (f.CreateUserId.Equals(loginUser.Id) || f.ActivityName == "确认报价单")).Select(f => f.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => (flowInstanceIds.Contains(q.FlowInstanceId) || (string.IsNullOrWhiteSpace(q.FlowInstanceId) && q.QuotationStatus >= 6)) && q.CreateUserId.Equals(loginUser.Id));
                                break;
                        }
                    }

                }
                else if (request.PageStart != null && request.PageStart == 2)
                {
                    if (loginUserRole.Any(r => r.Name.Equals("物料财务")))
                    {
                        switch (request.StartType)
                        {
                            case 1:
                                flowInstanceIds = flowinstanceList.Where(f => f.ActivityName == "财务审批").Select(f => f.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) || (string.IsNullOrWhiteSpace(q.FlowInstanceId) && q.QuotationStatus == 8));
                                break;
                            case 2:
                                flowInstanceIds = flowinstanceList.Where(f => f.CreateUserId.Equals(loginUser.Id) && f.Content == "财务审批").Select(f => f.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) || (string.IsNullOrWhiteSpace(q.FlowInstanceId) && q.QuotationStatus > 8));
                                break;
                            default:
                                flowInstanceIds = flowinstanceList.Where(f => (f.CreateUserId.Equals(loginUser.Id) || f.ActivityName == "财务审批")).Select(f => f.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) || (string.IsNullOrWhiteSpace(q.FlowInstanceId) && q.QuotationStatus >= 8));
                                break;
                        }
                    }
                    else
                    {
                        switch (request.StartType)
                        {
                            case 1:
                                flowInstanceIds = flowinstanceList.Where(f => f.ActivityName == "回传销售订单").Select(f => f.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => (flowInstanceIds.Contains(q.FlowInstanceId) || (string.IsNullOrWhiteSpace(q.FlowInstanceId) && q.QuotationStatus == 7)) && q.CreateUserId.Equals(loginUser.Id));
                                break;

                            case 2:
                                flowInstanceIds = flowinstanceList.Where(f => (f.CreateUserId.Equals(loginUser.Id) && f.Content == "回传销售订单")).Select(f => f.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => (flowInstanceIds.Contains(q.FlowInstanceId) || (string.IsNullOrWhiteSpace(q.FlowInstanceId) && q.QuotationStatus > 7)) && q.CreateUserId.Equals(loginUser.Id));
                                break;
                            default:
                                flowInstanceIds = flowinstanceList.Where(f => (f.CreateUserId.Equals(loginUser.Id) || f.ActivityName == "回传销售订单")).Select(f => f.Id).Distinct().ToList();
                                Quotations = Quotations.Where(q => (flowInstanceIds.Contains(q.FlowInstanceId) || (string.IsNullOrWhiteSpace(q.FlowInstanceId) && q.QuotationStatus >= 7)) && q.CreateUserId.Equals(loginUser.Id));
                                break;
                        }
                    }
                }
                else if (request.PageStart != null && request.PageStart == 3)
                {
                    if (!loginUserRole.Any(r => r.Name.Equals("仓库")))
                    {
                        if (loginUserRole.Any(r => r.Name.Equals("售后主管")))//售后主管查看其部门下所以人的数据
                        {
                            var orgId = loginContext.Orgs.OrderByDescending(c => c.CascadeId).FirstOrDefault()?.Id;
                            var orgUserIds = await UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.SecondId == orgId && c.Key == Define.USERORG).Select(c => c.FirstId).ToListAsync();
                            Quotations = Quotations.Where(q => orgUserIds.Contains(q.CreateUserId));
                        }
                        else
                        {
                            Quotations = Quotations.Where(q => q.CreateUserId.Equals(loginUser.Id));
                        }
                    }
                    switch (request.StartType)
                    {
                        case 1:
                            flowInstanceIds = flowinstanceList.Where(f => f.ActivityName == "待出库").Select(f => f.Id).Distinct().ToList();
                            Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) || (string.IsNullOrWhiteSpace(q.FlowInstanceId) && q.QuotationStatus == 10));
                            break;
                        case 2:
                            flowInstanceIds = flowinstanceList.Where(f => f.ActivityName == "结束").Select(f => f.Id).Distinct().ToList();
                            Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) || (string.IsNullOrWhiteSpace(q.FlowInstanceId) && q.QuotationStatus == 11));
                            break;
                        default:
                            flowInstanceIds = flowinstanceList.Where(f => f.ActivityName == "待出库" || f.ActivityName == "结束").Select(f => f.Id).Distinct().ToList();
                            Quotations = Quotations.Where(q => flowInstanceIds.Contains(q.FlowInstanceId) || (string.IsNullOrWhiteSpace(q.FlowInstanceId) && q.QuotationStatus >= 10));
                            break;
                    }
                    Quotations = Quotations.Where(q => (q.IsMaterialType != null || q.QuotationStatus == 11));
                }
                else
                {
                    if (!loginUserRole.Any(r => r.Name.Equals("物料稽查")))
                    {
                        if (loginUserRole.Any(r => r.Name.Equals("售后主管")))//售后主管查看其部门下所以人的数据
                        {
                            var orgId = loginContext.Orgs.OrderByDescending(c => c.CascadeId).FirstOrDefault()?.Id;
                            var orgUserIds = await UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.SecondId == orgId && c.Key == Define.USERORG).Select(c => c.FirstId).ToListAsync();
                            Quotations = Quotations.Where(q => orgUserIds.Contains(q.CreateUserId));
                        }
                        else
                        {
                            Quotations = Quotations.Where(q => q.CreateUserId.Equals(loginUser.Id));
                        }
                    }
                }
            }

            var QuotationDate = await Quotations.OrderByDescending(q => q.UpDateTime).Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();
            List<string> fileids = new List<string>();
            QuotationDate.ForEach(q => fileids.AddRange(q.QuotationPictures.Select(p => p.PictureId).ToList()));

            var file = await UnitWork.Find<UploadFile>(f => fileids.Contains(f.Id)).ToListAsync();
            ServiceOrderids = QuotationDate.Select(q => q.ServiceOrderId).ToList();
            var ServiceOrders = await UnitWork.Find<ServiceOrder>(null).Where(q => ServiceOrderids.Contains(q.Id)).Select(s => new { s.Id, s.TerminalCustomer, s.TerminalCustomerId, s.CustomerId, s.SalesMan, s.SalesManId }).ToListAsync();
            //提成状态
            var saleOrderId = QuotationDate.Select(c => c.SalesOrderId).ToList();
            var commsion = await UnitWork.Find<CommissionOrder>(c => saleOrderId.Contains(c.SalesOrderId)).Select(c => new { c.SalesOrderId, c.Status }).ToListAsync();

            var query = from a in QuotationDate
                        join b in ServiceOrders on a.ServiceOrderId equals b.Id
                        select new { a, b };
            var terminalCustomerIds = query.Select(q => q.b.TerminalCustomerId).ToList();
            var ocrds = await UnitWork.Find<OCRD>(o => terminalCustomerIds.Contains(o.CardCode)).Select(c => new { c.CardCode, c.Balance }).ToListAsync();
            var userIds = query.Select(q => q.a.CreateUserId).ToList();
            var SelOrgName = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(null).Select(o => new { o.Id, o.Name, o.CascadeId }).ToListAsync();
            var Relevances = await UnitWork.Find<Relevance>(r => r.Key == Define.USERORG && userIds.Contains(r.FirstId)).Select(r => new { r.FirstId, r.SecondId }).ToListAsync();
            var independentOrg = new string[] { "CS7", "CS12", "CS14", "CS17", "CS20", "CS29", "CS32", "CS34", "CS36", "CS37", "CS38", "CS9", "CS50", "CSYH" };

            result.Data = query.Select(q =>
            {
                var isfinish = flowinstanceObjs.Where(f => f.Id.Equals(q.a.FlowInstanceId)).FirstOrDefault()?.IsFinish;
                var orgName = SelOrgName.Where(s => s.Id.Equals(Relevances.Where(r => r.FirstId.Equals(q.a.CreateUserId)).FirstOrDefault()?.SecondId)).FirstOrDefault()?.Name;
                //提成状态
                var status = commsion.Where(c => c.SalesOrderId == q.a.SalesOrderId).FirstOrDefault()?.Status;
                return new
                {
                    q.a.Id,
                    q.a.ServiceOrderSapId,
                    q.a.ServiceOrderId,
                    q.b.TerminalCustomer,
                    q.b.CustomerId,
                    q.b.TerminalCustomerId,
                    q.a.TotalMoney,
                    TotalCommission = q.a.TotalCommission == null ? 0 : q.a.TotalCommission,
                    CommissionStatus = status,
                    //CreateUser = SelOrgName.Where(s => s.Id.Equals(Relevances.Where(r => r.FirstId.Equals(q.a.CreateUserId)).FirstOrDefault()?.SecondId)).FirstOrDefault()?.Name == null ? q.a.CreateUser : SelOrgName.Where(s => s.Id.Equals(Relevances.Where(r => r.FirstId.Equals(q.a.CreateUserId)).FirstOrDefault()?.SecondId)).FirstOrDefault()?.Name + "-" + q.a.CreateUser,
                    CreateUser = orgName == null ? q.a.CreateUser : orgName + "-" + q.a.CreateUser,
                    CreateUserId = q.a.CreateUserId,
                    IsContracting = orgName == null ? 0 : independentOrg.Contains(orgName) ? 1 : 0,
                    q.a.Remark,
                    q.a.SalesOrderId,
                    q.a.IsMaterialType,
                    CreateTime = Convert.ToDateTime(q.a.CreateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                    UpDateTime = q.a.QuotationOperationHistorys.FirstOrDefault() != null ? Convert.ToDateTime(q.a.QuotationOperationHistorys.OrderByDescending(h => h.CreateTime).FirstOrDefault()?.CreateTime).ToString("yyyy.MM.dd HH:mm:ss") : Convert.ToDateTime(q.a.UpDateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                    q.a.QuotationStatus,
                    //StatusName = string.IsNullOrWhiteSpace(q.a.FlowInstanceId) || q.a.CreateTime <= DateTime.Parse("2021-08-13") ? StatusName(q.a.QuotationStatus) : q.a.QuotationStatus == -1 ? "已取消" : flowinstanceObjs.Where(f => f.Id.Equals(q.a.FlowInstanceId)).FirstOrDefault()?.IsFinish == FlowInstanceStatus.Finished ? "已出库" : q.a.QuotationStatus == 12 ? "部分出库" : flowinstanceObjs.Where(f => f.Id.Equals(q.a.FlowInstanceId)).FirstOrDefault()?.IsFinish == FlowInstanceStatus.Rejected ? "驳回" : flowinstanceObjs.Where(f => f.Id.Equals(q.a.FlowInstanceId)).FirstOrDefault()?.IsFinish == null || q.a.IsDraft == true ? "未提交" : flowinstanceObjs.Where(f => f.Id.Equals(q.a.FlowInstanceId)).FirstOrDefault()?.IsFinish == FlowInstanceStatus.Draft ? "撤回" : flowinstanceObjs.Where(f => f.Id.Equals(q.a.FlowInstanceId)).FirstOrDefault()?.ActivityName,
                    StatusName = string.IsNullOrWhiteSpace(q.a.FlowInstanceId) || q.a.CreateTime <= DateTime.Parse("2021-08-13") ? StatusName(q.a.QuotationStatus) : q.a.QuotationStatus == -1 ? "已取消" : isfinish == FlowInstanceStatus.Finished ? "已出库" : q.a.QuotationStatus == 12 ? "部分出库" : isfinish == FlowInstanceStatus.Rejected ? "驳回" : isfinish == null || q.a.IsDraft == true ? "未提交" : isfinish == FlowInstanceStatus.Draft ? "撤回" : flowinstanceObjs.Where(f => f.Id.Equals(q.a.FlowInstanceId)).FirstOrDefault()?.ActivityName,
                    q.a.Tentative,
                    q.a.IsProtected,
                    q.a.PrintWarehouse,
                    q.a.CancelRequest,
                    q.b.SalesManId,
                    SalesMan = SelOrgName.Where(s => s.Id.Equals(Relevances.Where(r => r.FirstId.Equals(q.b.SalesManId)).FirstOrDefault()?.SecondId)).FirstOrDefault()?.Name + "-" + q.b.SalesMan,
                    SalesManDept = SelOrgName.Where(s => s.Id.Equals(Relevances.Where(r => r.FirstId.Equals(q.b.SalesManId)).FirstOrDefault()?.SecondId)).FirstOrDefault()?.Name,
                    Balance = ocrds.Where(o => o.CardCode.Equals(q.b.TerminalCustomerId)).FirstOrDefault()?.Balance,
                    files = q.a.QuotationPictures.Select(p => new
                    {
                        fileName = file.Where(f => f.Id.Equals(p.PictureId)).FirstOrDefault()?.FileName,
                        fileType = file.Where(f => f.Id.Equals(p.PictureId)).FirstOrDefault()?.FileType,
                        fileId = p?.PictureId
                    }).ToList()
                };
            }).OrderByDescending(q => Convert.ToDateTime(q.UpDateTime)).ToList();
            result.Count = await Quotations.CountAsync();

            return result;
        }
        private string StatusName(decimal? QuotationStatus)
        {
            var CategoryList = UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_QuotationStatus")).Select(u => new { u.DtValue, u.Name }).ToList();
            return CategoryList.Where(c => decimal.Parse(c.DtValue) == QuotationStatus).FirstOrDefault()?.Name;
        }
        /// <summary>
        /// 加载状态列表
        /// </summary>
        public async Task<TableData> GetQuotationOperationHistory(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var Quotations = await UnitWork.Find<Quotation>(null).Include(q => q.Expressages).Include(q => q.QuotationOperationHistorys).WhereIf(!string.IsNullOrWhiteSpace(request.ServiceOrderId.ToString()), q => q.ServiceOrderId.Equals(request.ServiceOrderId))
                     .WhereIf(!string.IsNullOrWhiteSpace(request.QuotationId.ToString()), q => q.Id.Equals(request.QuotationId)).ToListAsync();
            Quotations.ForEach(q => q.QuotationOperationHistorys = q.QuotationOperationHistorys.OrderBy(o => o.CreateTime).ToList());
            var serviceOrderIds = Quotations.Select(c => c.ServiceOrderId).ToList();
            var serivceOrder = await UnitWork.Find<ServiceOrder>(c => serviceOrderIds.Contains(c.Id)).Select(c => new { c.Id, c.TerminalCustomerId, c.TerminalCustomer }).ToListAsync();
            result.Data = Quotations.Skip((request.page - 1) * request.limit).Take(request.limit).Select(c => new
            {
                c.Id,
                c.ServiceOrderSapId,
                c.SalesOrderId,
                TerminalCustomerId = serivceOrder.Where(s => s.Id == c.ServiceOrderId).FirstOrDefault()?.TerminalCustomerId,
                TerminalCustomer = serivceOrder.Where(s => s.Id == c.ServiceOrderId).FirstOrDefault()?.TerminalCustomer,
                c.AcquisitionWay,
                c.CreateTime,
                c.UpDateTime,
                c.TotalMoney,
                c.Remark,
                QuotationOperationHistorys = c.QuotationOperationHistorys,
                Expressages = c.Expressages
            }).ToList();
            result.Count = Quotations.Count();
            return result;
        }

        /// <summary>
        /// 是否有更换类型物料未退料
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<bool> IsReturnMaterial(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = false;
            var quotation = await UnitWork.Find<Quotation>(c => c.ServiceOrderSapId == request.ServiceOrderSapId).Include(c => c.QuotationProducts).ThenInclude(c => c.QuotationMaterials).Where(c => c.QuotationProducts.Any(a => a.QuotationMaterials.Any(m => m.MaterialType == 1))).ToListAsync();
            if (quotation.Count==0)
                return result;
            if (quotation.All(c=>c.SalesOrderId!=null))
            {
                var saleOrderIds = quotation.Select(c => c.SalesOrderId).ToList();
                var returnote = await UnitWork.Find<ReturnNote>(c => saleOrderIds.Contains(c.SalesOrderId))
                    .Include(c => c.ReturnNoteProducts)
                    .ThenInclude(c => c.ReturnNoteMaterials)
                    .ToListAsync();
                quotation.ForEach(c =>
                {
                    //领料单下序列号
                    c.QuotationProducts.ForEach(q =>
                    {
                        var obj = returnote?.Where(r => r.SalesOrderId == c.SalesOrderId).FirstOrDefault();
                        var product = obj?.ReturnNoteProducts.Where(r => r.ProductCode == q.ProductCode).FirstOrDefault();//退料单序列号
                        q.QuotationMaterials.ForEach(m =>
                        {
                            result = false;//全部满足则为true
                            if (m.Count == product?.ReturnNoteMaterials.Where(nm => nm.MaterialCode == m.MaterialCode).Count())//领料单物料的数量等于退料物料的数量
                            {
                                result = true;
                            }
                        });
                    });
                });
            }

            return result;
        }
        /// <summary>
        /// 加载服务单列表
        /// </summary>
        public async Task<TableData> GetServiceOrderList(QueryQuotationListReq request)
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
            var result = new TableData();
            var ServiceOrders = from a in UnitWork.Find<ServiceOrder>(s => s.VestInOrg <= 1 && s.AllowOrNot == 0)
                                join b in UnitWork.Find<ServiceWorkOrder>(s => s.Status < 7) on a.Id equals b.ServiceOrderId
                                select new { a, b };
            ServiceOrders = ServiceOrders.WhereIf(!string.IsNullOrWhiteSpace(request.ServiceOrderId.ToString()), s => s.a.Id.Equals(request.ServiceOrderId))
                .WhereIf(!string.IsNullOrWhiteSpace(request.ServiceOrderSapId.ToString()), s => s.a.U_SAP_ID.Equals(request.ServiceOrderSapId));
            var ServiceOrderList = (await ServiceOrders.Where(s => s.b.CurrentUserNsapId.Equals(loginUser.Id)).ToListAsync()).GroupBy(s => s.a.Id).Select(s => s.First());
            var CustomerIds = ServiceOrderList.Select(s => s.a.TerminalCustomerId).ToList();
            var Address = await CardAddress(CustomerIds);
            result.Data = ServiceOrderList.Skip((request.page - 1) * request.limit)
                .Take(request.limit).Select(q => new
                {
                    q.a.Id,
                    q.a.U_SAP_ID,
                    q.a.TerminalCustomer,
                    q.a.TerminalCustomerId,
                    q.a.NewestContacter,
                    q.a.NewestContactTel,
                    q.b.FromTheme,
                    q.a.SalesMan,
                    q.a.FromId,
                    BillingAddress = Address.Where(a => a.CardCode.Equals(q.a.CustomerId)).FirstOrDefault()?.BillingAddress,//开票地址
                    DeliveryAddress = Address.Where(a => a.CardCode.Equals(q.a.CustomerId)).FirstOrDefault()?.DeliveryAddress, //收货地址
                    Balance = Address.Where(a => a.CardCode.Equals(q.a.CustomerId)).FirstOrDefault()?.Balance, //额度
                    frozenFor = Address.Where(a => a.CardCode.Equals(q.a.CustomerId)).FirstOrDefault()?.frozenFor == "N" ? "正常" : "冻结" //客户状态
                });
            result.Count = ServiceOrderList.Count();

            return result;
        }
        /// <summary>
        /// 获取客户地址
        /// </summary>
        /// <param name="CardCodes"></param>
        /// <returns></returns>
        public async Task<List<CardAddressResp>> CardAddress(List<string> CardCodes)
        {
            var CardAddress = from a in UnitWork.Find<OCRD>(null)
                              join c in UnitWork.Find<OCRY>(null) on a.Country equals c.Code into ac
                              from c in ac.DefaultIfEmpty()
                              join d in UnitWork.Find<OCST>(null) on a.State1 equals d.Code into ad
                              from d in ad.DefaultIfEmpty()
                              join e in UnitWork.Find<OCRY>(null) on a.MailCountr equals e.Code into ae
                              from e in ae.DefaultIfEmpty()
                              where CardCodes.Contains(a.CardCode)
                              select new { a, c, d, e };
            var Address = await CardAddress.Select(q => new CardAddressResp
            {
                CardCode= q.a.CardCode,
                Balance=q.a.Balance,
                frozenFor=q.a.frozenFor,
                BillingAddress = $"{ q.a.ZipCode ?? "" }{ q.c.Name ?? "" }{ q.d.Name ?? "" }{ q.a.City ?? ""}{ q.a.Building ?? "" }",
                DeliveryAddress = $"{ q.a.MailZipCod ?? "" }{ q.e.Name ?? "" }{ q.d.Name ?? "" }{ q.a.MailCity ?? "" }{ q.a.MailBuildi ?? "" }"
            }).ToListAsync();
            return Address;
        }
        /// <summary>
        /// 获取序列号和设备
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> GetSerialNumberList(QueryQuotationListReq request)
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
            var result = new TableData();

            var ServiceWorkOrderList = await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId.Equals(request.ServiceOrderId))
                .WhereIf(string.IsNullOrWhiteSpace(request.CreateUserId), s => s.CurrentUserNsapId.Equals(loginUser.Id))
                .WhereIf(!string.IsNullOrWhiteSpace(request.CreateUserId), s => s.CurrentUserNsapId.Equals(request.CreateUserId))
                .WhereIf(!string.IsNullOrWhiteSpace(request.MaterialType), s => s.MaterialCode.Substring(0, 2) == request.MaterialType)
                .WhereIf(!string.IsNullOrWhiteSpace(request.ManufacturerSerialNumbers), s => s.ManufacturerSerialNumber.Contains(request.ManufacturerSerialNumbers))
                .WhereIf(!string.IsNullOrWhiteSpace(request.MaterialCode), s => s.MaterialCode.Contains(request.MaterialCode))
                .Select(s => new { s.ManufacturerSerialNumber, s.MaterialCode, s.MaterialDescription, s.FromTheme }).ToListAsync();
            if (ServiceWorkOrderList != null && ServiceWorkOrderList.Count > 0)
            {
                #region 获取交货创建时间
                var mnfSerials = ServiceWorkOrderList.Select(s => s.ManufacturerSerialNumber).ToList();

                var manufacturerSerialNumber = from a in UnitWork.Find<OITL>(null)
                                               join b in UnitWork.Find<ITL1>(null) on new { a.LogEntry, a.ItemCode } equals new { b.LogEntry, b.ItemCode } into ab
                                               from b in ab.DefaultIfEmpty()
                                               join c in UnitWork.Find<OSRN>(null) on new { b.ItemCode, SysNumber = b.SysNumber.Value } equals new { c.ItemCode, c.SysNumber } into bc
                                               from c in bc.DefaultIfEmpty()
                                               where (a.DocType == 15 || a.DocType == 59) && mnfSerials.Contains(c.MnfSerial)
                                               select new { c.MnfSerial, a.DocEntry, a.BaseEntry, a.DocType, a.CreateDate, a.BaseType };
                #region 暂时废弃
                //var Equipments = from a in manufacturerSerialNumber
                //                 join b in UnitWork.Find<ODLN>(null) on a.DocEntry equals b.DocEntry into ab
                //                 from b in ab.DefaultIfEmpty()
                //                 select new { a.DocEntry, a.MnfSerial };
                //var EquipmentList = await Equipments.ToListAsync();
                //  var Equipments = await UnitWork.Query<SysEquipmentColumn>(@$"SELECT d.DocEntry,c.MnfSerial
                //      FROM oitl a left join itl1 b
                //      on a.LogEntry = b.LogEntry and a.ItemCode = b.ItemCode 
                //      left join osrn c on b.ItemCode = c.ItemCode and b.SysNumber = c.SysNumber
                //left join odln d on a.DocEntry=d.DocEntry
                //      where a.DocType =15 and c.MnfSerial in ({MnfSerialStr.ToString().Substring(0, MnfSerialStr.Length - 1)})").Select(s => new SysEquipmentColumn { MnfSerial = s.MnfSerial, DocEntry = s.DocEntry }).ToListAsync();

                //var DocEntryIds = Equipments.Select(e => e.DocEntry).ToList();

                //var buyopors = from a in UnitWork.Find<buy_opor>(null)
                //               join b in UnitWork.Find<sale_transport>(null) on a.DocEntry equals b.Buy_DocEntry
                //               where DocEntryIds.Contains(b.Base_DocEntry) && b.Base_DocType == 24
                //               select new { a, b };
                #endregion

                var MnfSerialList = await manufacturerSerialNumber.ToListAsync();
                var docdate = await manufacturerSerialNumber.Where(m => m.BaseType == 17).ToListAsync();
                var SalesOrderWarrantyDates = await UnitWork.Find<SalesOrderWarrantyDate>(s => mnfSerials.Contains(s.MnfSerial)).ToListAsync();
                var IsProtecteds = docdate.Select(e => new
                {
                    MnfSerial = e.MnfSerial,
                    DocDate = SalesOrderWarrantyDates.Where(s => s.MnfSerial.Equals(e.MnfSerial)).Count() > 0 ? SalesOrderWarrantyDates.Where(s => s.MnfSerial.Equals(e.MnfSerial)).FirstOrDefault()?.WarrantyPeriod : Convert.ToDateTime(e.CreateDate).AddMonths(13)
                }).ToList();
                #endregion
                List<QuotationProduct> quotationProducts = new List<QuotationProduct>();
                if (!string.IsNullOrWhiteSpace(request.QuotationId.ToString()))
                {
                    quotationProducts = await UnitWork.Find<QuotationProduct>(q => q.QuotationId == request.QuotationId).ToListAsync();
                }
                var data = ServiceWorkOrderList.Select(s => new ProductCodeListResp
                {
                    SalesOrder = MnfSerialList.Where(m => m.MnfSerial.Equals(s.ManufacturerSerialNumber) && m.BaseType == 17)?.Max(m => m.BaseEntry),
                    ProductionOrder = MnfSerialList.Where(m => m.MnfSerial.Equals(s.ManufacturerSerialNumber) && m.BaseType == 202)?.Max(m => m.BaseEntry),
                    ManufacturerSerialNumber = s.ManufacturerSerialNumber,
                    MaterialCode = s.MaterialCode,
                    MaterialDescription = s.MaterialDescription,
                    IsProtected = IsProtecteds.Where(i => i.MnfSerial.Equals(s.ManufacturerSerialNumber)).FirstOrDefault()?.DocDate > DateTime.Now ? true : false,
                    DocDate = IsProtecteds.Where(i => i.MnfSerial.Equals(s.ManufacturerSerialNumber)).OrderByDescending(s => s.DocDate).FirstOrDefault()?.DocDate == null ? SalesOrderWarrantyDates.Where(w => w.MnfSerial.Equals(s.ManufacturerSerialNumber)).FirstOrDefault()?.WarrantyPeriod : IsProtecteds.Where(i => i.MnfSerial.Equals(s.ManufacturerSerialNumber)).OrderByDescending(s => s.DocDate).FirstOrDefault()?.DocDate,
                    FromTheme = s.FromTheme,
                    WarrantyTime = quotationProducts.Where(q => q.ProductCode.Equals(s.ManufacturerSerialNumber)).FirstOrDefault()?.WarrantyTime
                }).OrderBy(s => s.MaterialCode).ToList();
                if (request.FromId == 8 )
                {
                    var listCode = data.Select(a => a.MaterialCode).Distinct().ToList();
                    var listMaterialDetial =await GetMaterialDetial((int)request.ServiceOrderId, listCode);
                    data.ForEach(item =>
                    item.listMaterial = listMaterialDetial.Where(a => a.MnfSerial == item.MaterialCode).ToList()
                    ) ;
                }
                result.Data = data;
            }
            result.Count = ServiceWorkOrderList.Count();
            return result;
        }

        /// <summary>
        /// 获取物料列表
        /// </summary>
        public async Task<TableData> GetMaterialCodeList(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var Equipments = await EquipmentList(request);
            var quotations = await UnitWork.Find<Quotation>(q => q.ServiceOrderId.Equals(request.ServiceOrderId)).Include(q => q.QuotationProducts).ThenInclude(q => q.QuotationMaterials).ToListAsync();
            if (quotations != null && quotations.Count > 0)
            {
                List<QuotationMaterial> quotationMaterials = new List<QuotationMaterial>();
                quotations.ForEach(q =>
                    q.QuotationProducts.Where(p => p.ProductCode.Equals(request.ManufacturerSerialNumbers)).ForEach(p =>
                        quotationMaterials.AddRange(p.QuotationMaterials.ToList())
                    )
                );
                Equipments.ForEach(e =>
                      e.Quantity = e.Quantity - quotationMaterials.Where(q => q.MaterialCode.Equals(e.ItemCode)).Sum(q => q.Count)
                );
            }
            Equipments = Equipments.Where(e => e.Quantity > 0).ToList();
            var ItemCodes = Equipments.Select(e => e.ItemCode).ToList();
            var MaterialPrices = await UnitWork.Find<MaterialPrice>(m => ItemCodes.Contains(m.MaterialCode)).ToListAsync();

            //var categoryList = await UnitWork.Find<Category>(c => c.TypeId.Equals("SYS_WarehouseMaterial")).Select(c=>c.Name).ToListAsync();
            var EquipmentsList = Equipments.Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToList();
            EquipmentsList.ForEach(e =>
            {
                e.MnfSerial = request.ManufacturerSerialNumbers;
                var Prices = MaterialPrices.Where(m => m.MaterialCode.Equals(e.ItemCode)).FirstOrDefault();
                //4.0存在物料价格，取4.0的价格为售后结算价，销售价取售后结算价*销售价倍数，不存在就当前进货价*1.2 为售后结算价。销售价为售后结算价*3
                if (Prices != null)
                {
                    e.UnitPrice = Prices?.SettlementPrice == null || Prices?.SettlementPrice <= 0 ? e.lastPurPrc * Prices?.SettlementPriceModel : Prices?.SettlementPrice;
                    //var s = e.UnitPrice.ToDouble().ToString();
                    //if (s.IndexOf(".") > 0)
                    //{
                    //    s = s.Substring(s.IndexOf("."), s.Length - s.IndexOf("."));
                    //    if (s.Length > 1)
                    //    {
                    //        int lengeth = s.Substring(1, s.Length - 1).Length;
                    //        if (lengeth > 3) e.UnitPrice = e.UnitPrice + 0.005M;
                    //    }
                    //}
                    e.UnitPrice = decimal.Parse(e.UnitPrice.ToString("#0.0000"));
                    e.lastPurPrc = e.UnitPrice * Prices.SalesMultiple;
                }
                else
                {
                    e.UnitPrice = e.lastPurPrc * 1.2M;
                    //var s = e.UnitPrice.ToDouble().ToString();
                    //if (s.IndexOf(".") > 0)
                    //{
                    //    s = s.Substring(s.IndexOf("."), s.Length - s.IndexOf("."));
                    //    if (s.Length > 1)
                    //    {
                    //        int lengeth = s.Substring(1, s.Length - 1).Length;
                    //        if (lengeth > 3) e.UnitPrice = e.UnitPrice + 0.005M;
                    //    }
                    //}
                    e.UnitPrice = decimal.Parse(e.UnitPrice.ToString("#0.0000"));
                    e.lastPurPrc = e.UnitPrice * 3;
                    //A6开头 不带机箱、机柜的物料 销售价为结算价的2倍
                    if (e.ItemCode.StartsWith("A6") && !(e.ItemName.Contains("机柜") || e.ItemName.Contains("机箱")))
                        e.lastPurPrc = e.UnitPrice * 2;
                    if (e.ItemCode == "A801-7'S-CC-WX")
                        e.lastPurPrc = e.UnitPrice * 2;
                }

            });

            result.Data = EquipmentsList;
            result.Count = Equipments.Count();
            return result;
        }

        /// <summary>
        /// 通用获取物料列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task<List<SysEquipmentColumn>> EquipmentList(QueryQuotationListReq request)
        {
            SqlParameter[] parameter = new SqlParameter[]
            {
               new SqlParameter("ManufacturerSerialNumbers", request.ManufacturerSerialNumbers),
               new SqlParameter("WhsCode", request.WhsCode)
            };
            var baseEntryList = await UnitWork.Query<SysEquipmentColumn>(@$"SELECT a.BaseEntry FROM oitl a left join itl1 b
                    on a.LogEntry = b.LogEntry and a.ItemCode = b.ItemCode 
                    left join osrn c on b.ItemCode = c.ItemCode and b.SysNumber = c.SysNumber
                    where a.DocType in (15, 59) and c.MnfSerial = @ManufacturerSerialNumbers and a.BaseType=202", parameter)
                    .Select(s => s.BaseEntry.ToString()).Distinct().ToListAsync();
            var baseEntrys = await UnitWork.Find<product_owor>(p => baseEntryList.Contains(p.CDocEntry)).Select(p => p.DocEntry.ToString()).ToListAsync();
            baseEntryList.AddRange(baseEntrys);
            var Equipments = await UnitWork.Query<SysEquipmentColumn>(@$"select a.DocEntry,a.ItemCode,c.ItemName,c.BuyUnitMsr,d.OnHand, d.WhsCode,a.BaseQty as Quantity ,c.lastPurPrc from WOR1 a 
						join OITM c on a.itemcode = c.itemcode
						join OITW d on a.itemcode=d.itemcode 
						where d.WhsCode= @WhsCode", parameter).WhereIf(!string.IsNullOrWhiteSpace(request.PartCode), s => s.ItemCode.Contains(request.PartCode))
                        .WhereIf(!string.IsNullOrWhiteSpace(request.PartDescribe), s => s.ItemName.Contains(request.PartDescribe))
                        .WhereIf(!string.IsNullOrWhiteSpace(request.AppPartCode),s=> s.ItemName.Contains(request.AppPartCode) || s.ItemCode.Contains(request.AppPartCode))
                        .Where(s=> baseEntryList.Contains(s.DocEntry.ToString())).Select(s => new SysEquipmentColumn { ItemCode = s.ItemCode, MnfSerial = request.ManufacturerSerialNumbers, ItemName = s.ItemName, BuyUnitMsr = s.BuyUnitMsr, OnHand = s.OnHand, WhsCode = s.WhsCode, Quantity = s.Quantity, lastPurPrc = s.lastPurPrc }).ToListAsync();

            if (Equipments == null || Equipments.Count() <= 0)
            {
                request.MaterialCode = request.MaterialCode.Replace("'", "''");
                parameter = new SqlParameter[]
                {
                   new SqlParameter("MaterialCode", request.MaterialCode),
                   new SqlParameter("WhsCode", request.WhsCode)
                };

                //select a.* ,c.lastPurPrc from(select a.Father as MnfSerial,a.Code as ItemCode,a.U_Desc as ItemName,a.U_DUnit as BuyUnitMsr,b.OnHand,b.WhsCode,a.Quantity
                //       from ITT1 a join OITW b on a.Code = b.ItemCode  where a.Father = @MaterialCode and b.WhsCode = @WhsCode) a join OITM c on c.ItemCode = a.ItemCode
                Equipments = await UnitWork.Query<SysEquipmentColumn>(@$"select a.* ,c.lastPurPrc from (select a.Father as MnfSerial,a.Code as ItemCode,c.ItemName,a.U_DUnit as BuyUnitMsr,d.OnHand,d.WhsCode,a.Quantity
                        from ITT1 a
	                        JOIN OITM c ON a.code = c.itemcode
	                        JOIN OITW d ON a.code= d.itemcode  where a.Father=@MaterialCode and d.WhsCode=@WhsCode) a join OITM c on c.ItemCode=a.ItemCode", parameter)
                    .WhereIf(!string.IsNullOrWhiteSpace(request.PartCode), s => s.ItemCode.Contains(request.PartCode))
                    .WhereIf(!string.IsNullOrWhiteSpace(request.PartDescribe), s => request.PartDescribe.Contains(s.ItemName))
                    .Select(s => new SysEquipmentColumn { ItemCode = s.ItemCode, MnfSerial = s.MnfSerial, ItemName = s.ItemName, BuyUnitMsr = s.BuyUnitMsr, OnHand = s.OnHand, WhsCode = s.WhsCode, Quantity = s.Quantity, lastPurPrc = s.lastPurPrc }).ToListAsync();
            }
            else
            {
                List<SysEquipmentColumn> EquipmentsObj = new List<SysEquipmentColumn>();
                Equipments.GroupBy(e => e.ItemCode).Select(e=>new { obj=e.First(),count=e.Sum(s=>s.Quantity) }).ForEach(e => 
                    {
                        e.obj.Quantity = e.count;
                        EquipmentsObj.Add(e.obj);
                    }
                );
                Equipments = EquipmentsObj;
            }
            if (request.IsWarranty == null || (bool)request.IsWarranty == false)
            {
                var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ShieldingMaterials")).Select(u => u.Name).ToListAsync();

                Equipments = Equipments.Where(e => !CategoryList.Contains(e.ItemCode)).ToList();
            }
            else
            {
                Equipments = Equipments.Where(e => e.ItemCode.Equals("S111-SERVICE-YB")).ToList();
            }
            return Equipments;
        }

        /// <summary>
        /// 查询物料剩余库存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> GetMaterialCodeOnHand(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var materialCodeOnHand = (await UnitWork.Find<OITW>(o => o.ItemCode.Equals(request.MaterialCode) && o.WhsCode.Equals(request.WhsCode)).FirstOrDefaultAsync())?.OnHand;
            result.Data = new { OnHand = materialCodeOnHand };
            return result;
        }

        /// <summary>
        /// 获取物料仓库与库存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> GetMaterialOnHand(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var query = await UnitWork.Find<OITW>(o => o.ItemCode == request.MaterialCode && o.OnHand > 0)
                .WhereIf(!string.IsNullOrWhiteSpace(request.WhsCode), c => c.WhsCode == request.WhsCode)
                .Select(c => new { c.ItemCode, c.OnHand, c.WhsCode })
                .ToListAsync();
            return new TableData
            {
                Data = query
            };
        }

        /// <summary>
        /// 获取所有物料
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> GetMaterial(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var query = UnitWork.Find<OITM>(c => c.ItemCode.Contains(request.ItemCode)).Select(c => new { c.ItemCode, c.ItemName });
            result.Count = await query.CountAsync();
            result.Data=await query.Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();
            return result;
        }

        /// <summary>
        /// 获取报价单详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> GetDetails(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var Quotations = await GeneralDetails((int)request.QuotationId, request.IsUpdate);
            //var CreaterOrgInfo = await _userManagerApp.GetUserOrgInfo(Quotations.CreateUserId);
            //Quotations.CreateUser = CreaterOrgInfo != null ? CreaterOrgInfo.OrgName + "-" + Quotations.CreateUser : Quotations.CreateUser;

            var ServiceOrders = await UnitWork.Find<ServiceOrder>(s => s.Id.Equals(Quotations.ServiceOrderId)).Include(s => s.ServiceWorkOrders).FirstOrDefaultAsync();
            //为职员加上部门前缀
            var recepUserOrgInfo = await _userManagerApp.GetUserOrgInfo(ServiceOrders.RecepUserId);
            ServiceOrders.RecepUserDept = recepUserOrgInfo != null ? recepUserOrgInfo.OrgName : "";
            var salesManOrgInfo = await _userManagerApp.GetUserOrgInfo(ServiceOrders.SalesManId);
            ServiceOrders.SalesManDept = salesManOrgInfo != null ? salesManOrgInfo.OrgName : "";
            var superVisorOrgInfo = await _userManagerApp.GetUserOrgInfo(ServiceOrders.SupervisorId);
            ServiceOrders.SuperVisorDept = superVisorOrgInfo != null ? superVisorOrgInfo.OrgName : "";

            var CustomerInformation = await UnitWork.Find<OCRD>(o => o.CardCode.Equals(ServiceOrders.TerminalCustomerId)).Select(o => new { frozenFor = o.frozenFor == "N" ? "正常" : "冻结",o.Balance}).FirstOrDefaultAsync();
            var QuotationMergeMaterials = await UnitWork.Find<QuotationMergeMaterial>(q => q.QuotationId.Equals(request.QuotationId)).ToListAsync();
            QuotationMergeMaterials = QuotationMergeMaterials.OrderBy(q => q.MaterialCode).ToList();
            Quotations.ServiceRelations = (await UnitWork.Find<User>(u => u.Id.Equals(Quotations.CreateUserId)).FirstOrDefaultAsync()).ServiceRelations;
            //var ocrds = await UnitWork.Find<OCRD>(o => ServiceOrders.TerminalCustomerId.Equals(o.CardCode)).FirstOrDefaultAsync();
            var result = new TableData();
            if (Quotations.Status == 2)
            {
                var ExpressageList = await UnitWork.Find<Expressage>(e => e.QuotationId.Equals(Quotations.Id)).Include(e => e.ExpressagePicture).Include(e => e.LogisticsRecords).ToListAsync();
                List<LogisticsRecord> LogisticsRecords = new List<LogisticsRecord>();

                var fileids = new List<string>();
                foreach (var item in ExpressageList)
                {
                    fileids.AddRange(item.ExpressagePicture.Select(p => p.PictureId).ToList());
                    LogisticsRecords.AddRange(item.LogisticsRecords.ToList());
                }

                var files = await UnitWork.Find<UploadFile>(f => fileids.Contains(f.Id)).ToListAsync();
                var MergeMaterials = from a in QuotationMergeMaterials
                                     join b in LogisticsRecords on a.Id equals b.QuotationMaterialId
                                     select new { a, b };

                var Expressages = ExpressageList.Select(e => new
                {
                    ExpressagePicture = e.ExpressagePicture.Select(p => new
                    {
                        p.PictureId,
                        p.Id,
                        p.ExpressageId,
                        FileName = files.FirstOrDefault(f => f.Id.Equals(p.PictureId))?.FileName,
                        FileType = files.FirstOrDefault(f => f.Id.Equals(p.PictureId))?.FileType,
                    }),
                    e.ExpressInformation,
                    e.ExpressNumber,
                    e.Id,
                    e.Freight,
                    e.QuotationId,
                    e.Remark,
                    e.ReturnNoteId,
                    LogisticsRecords = MergeMaterials.Where(m => m.b.ExpressageId.Equals(e.Id)).Select(m => new
                    {
                        m.a.MaterialCode,
                        m.a.MaterialDescription,
                        m.a.Count,
                        m.a.Unit,
                        m.a.SentQuantity,
                        m.b.Quantity,
                        m.a.WhsCode
                    }).ToList()
                }).ToList();
                if (request.PageType == 1)//提成单详情
                {
                    var obj = await UnitWork.Find<CommissionOrder>(c => c.SalesOrderId == Quotations.SalesOrderId).FirstOrDefaultAsync();
                    //操作历史
                    var operationHistories = await UnitWork.Find<FlowInstanceOperationHistory>(c => c.InstanceId == obj.FlowInstanceId)
                        .OrderBy(c => c.CreateDate).Select(h => new
                        {
                            CreateTime = Convert.ToDateTime(h.CreateDate).ToString("yyyy.MM.dd HH:mm:ss"),
                            h.Remark,
                            IntervalTime = h.IntervalTime != null && h.IntervalTime > 0 ? h.IntervalTime / 60 : null,
                            h.CreateUserName,
                            h.Content,
                            h.ApprovalResult,
                        }).ToListAsync();
                    var CommissionOrder = new { obj.DocStatus, obj.BillStatus, obj.Receivables, operationHistories };
                    result.Data = new
                    {
                        Balance = CustomerInformation?.Balance,
                        Expressages,
                        Quotations = Quotations,
                        QuotationMergeMaterials,
                        ServiceOrders,
                        CustomerInformation,
                        CommissionOrder
                    };
                }
                else
                {
                    result.Data = new
                    {
                        Balance = CustomerInformation?.Balance,
                        Expressages,
                        Quotations = Quotations,
                        QuotationMergeMaterials,
                        ServiceOrders,
                        CustomerInformation
                    };
                }
            }
            else
            {
                result.Data = new
                {
                    Balance = CustomerInformation?.Balance,
                    Quotations = Quotations,
                    QuotationMergeMaterials,
                    ServiceOrders,
                    CustomerInformation
                };
            }
            return result;
        }

        /// <summary>
        /// 报价单详情操作
        /// </summary>
        /// <param name="QuotationId"></param>
        /// <param name="IsUpdate"></param>
        /// <returns></returns>
        public async Task<AddOrUpdateQuotationReq> GeneralDetails(int QuotationId, bool? IsUpdate)
        {
            var Quotations = await UnitWork.Find<Quotation>(q => q.Id == QuotationId).Include(q => q.QuotationPictures).Include(q => q.QuotationProducts).ThenInclude(q => q.QuotationMaterials).ThenInclude(q => q.QuotationMaterialPictures).Include(q => q.QuotationOperationHistorys).FirstOrDefaultAsync();
            var quotationsMap = Quotations.MapTo<AddOrUpdateQuotationReq>();
            List<string> materialCodes = new List<string>();
            List<string> WhsCode = new List<string>();
            Quotations.QuotationProducts.ForEach(q =>
            {
                WhsCode.AddRange(q.QuotationMaterials.Select(m => m.WhsCode).ToList());
                materialCodes.AddRange(q.QuotationMaterials.Select(m => m.MaterialCode).ToList());
            });
            var ItemCodes = await UnitWork.Find<OITW>(o => materialCodes.Contains(o.ItemCode) && WhsCode.Contains(o.WhsCode)).Select(o => new { o.ItemCode, o.WhsCode, o.OnHand }).ToListAsync();
            List<QuotationMaterialReq> quotationMaterials = new List<QuotationMaterialReq>();
            if (IsUpdate != null && (bool)IsUpdate)
            {
                var oITMS = await UnitWork.Find<OITM>(o => materialCodes.Contains(o.ItemCode)).Select(o => new QuotationMaterialReq { MaterialCode = o.ItemCode, SalesPrice = o.LastPurPrc }).ToListAsync();
                var MaterialPrices = await UnitWork.Find<MaterialPrice>(m => materialCodes.Contains(m.MaterialCode)).ToListAsync();
                oITMS.ForEach(o =>
                {
                    var Prices = MaterialPrices.Where(m => m.MaterialCode.Equals(o.MaterialCode)).FirstOrDefault();
                    //4.0存在物料价格，取4.0的价格为售后结算价，销售价取售后结算价*销售价倍数，不存在就当前进货价*1.2 为售后结算价。销售价为售后结算价*3
                    if (Prices != null)
                    {
                        o.UnitPrice = Prices?.SettlementPrice == null || Prices?.SettlementPrice <= 0 ? o.SalesPrice * Prices?.SettlementPriceModel : Prices?.SettlementPrice;

                        o.UnitPrice = decimal.Parse(o.UnitPrice.ToString("#0.0000"));
                        o.SalesPrice = o.UnitPrice * Prices.SalesMultiple;
                    }
                    else
                    {
                        o.UnitPrice = o.SalesPrice * 1.2M;
                        o.UnitPrice = decimal.Parse(o.UnitPrice.ToString("#0.0000"));
                        o.SalesPrice = o.UnitPrice * 3;
                    }
                });
                quotationMaterials.AddRange(oITMS.ToList());
            }
            quotationsMap.QuotationProducts.ForEach(p =>
                p.QuotationMaterials.ForEach(m =>
                {
                    m.WhsCode = m.WhsCode;
                    m.WarehouseQuantity = ItemCodes.Where(i => i.ItemCode.Equals(m.MaterialCode) && i.WhsCode.Equals(m.WhsCode)).FirstOrDefault()?.OnHand;
                    m.TotalPrice = m.TotalPrice == 0 && m.MaterialType != "3" && m.MaterialType != "4" ? decimal.Parse(Convert.ToDecimal((m.UnitPrice * 3 * (m.Discount / 100) * m.Count)).ToString("#0.00")) : m.TotalPrice;
                    m.SalesPrice = m.SalesPrice == 0 && m.MaterialType != "3" ? decimal.Parse(Convert.ToDecimal(m.UnitPrice * 3).ToString("#0.00")) : m.SalesPrice;
                    if (m.DiscountPrices < 0) m.DiscountPrices = m.SalesPrice == 0 && m.MaterialType != "3" && m.MaterialType != "3" ? decimal.Parse(Convert.ToDecimal(m.UnitPrice * 3 * (m.Discount / 100)).ToString("#0.00")) : decimal.Parse(Convert.ToDecimal(m.SalesPrice * (m.Discount / 100)).ToString("#0.00"));
                    if (IsUpdate != null && (bool)IsUpdate) m.UnitPrice = quotationMaterials.Where(q => q.MaterialCode.Equals(m.MaterialCode)).FirstOrDefault()?.UnitPrice;
                    if (IsUpdate != null && (bool)IsUpdate) m.SalesPrice = quotationMaterials.Where(q => q.MaterialCode.Equals(m.MaterialCode)).FirstOrDefault()?.SalesPrice;
                    if (IsUpdate != null && (bool)IsUpdate) m.Discount = m.MaterialType != "4" && m.MaterialType != "3" && m.SalesPrice > 0 ? Convert.ToDecimal(m.DiscountPrices / m.SalesPrice) * 100 : m.Discount;
                }
                )
            );

            var orgrole = await _orgApp.GetOrgNameAndRoleIdentity(quotationsMap.CreateUserId);
            quotationsMap.OrgName = orgrole.OrgName;
            quotationsMap.RoleIdentity = orgrole.RoleIdentity;

            List<QuotationMaterialReq> QuotationMergeMaterial = new List<QuotationMaterialReq>();
            List<ProductCodeListResp> serialNumberList = (await GetSerialNumberList(new QueryQuotationListReq { ServiceOrderId = quotationsMap.ServiceOrderId, CreateUserId = quotationsMap.CreateUserId })).Data;
            var count = 0;
            var isBool = (quotationsMap.ServiceChargeJH != null && quotationsMap.ServiceChargeJH > 0) || (quotationsMap.ServiceChargeJHCost != null && quotationsMap.ServiceChargeJHCost > 0);
            isBool = isBool == true ? true : (quotationsMap.ServiceChargeSM != null && quotationsMap.ServiceChargeSM > 0) || (quotationsMap.ServiceChargeSMCost != null && quotationsMap.ServiceChargeSMCost > 0);
            isBool = isBool == true ? true : (quotationsMap.TravelExpense != null && quotationsMap.TravelExpense > 0) || (quotationsMap.TravelExpenseCost != null && quotationsMap.TravelExpenseCost > 0);
            if (isBool && (IsUpdate == null || IsUpdate == false))
            {
                var productCodeList = quotationsMap.QuotationProducts.Select(q => q.ProductCode).ToList();
                var products = serialNumberList.Where(s => !productCodeList.Contains(s.ManufacturerSerialNumber)).Select(s => new QuotationProductReq
                {
                    MaterialCode = s.MaterialCode,
                    ProductCode = s.ManufacturerSerialNumber,
                    IsProtected = s.IsProtected,
                    MaterialDescription = s.MaterialDescription,
                    WarrantyExpirationTime = s.DocDate,
                    FromTheme = s.FromTheme,
                    QuotationMaterials = new List<QuotationMaterialReq>()
                }).ToList();
                quotationsMap.QuotationProducts.AddRange(products);
                count = quotationsMap.QuotationProducts.Count();
                if ((quotationsMap.ServiceChargeJH != null && quotationsMap.ServiceChargeJH > 0) || (quotationsMap.ServiceChargeJHCost != null && quotationsMap.ServiceChargeJHCost > 0))
                {
                    QuotationMergeMaterial.Add(new QuotationMaterialReq
                    {
                        Id = Guid.NewGuid().ToString(),
                        MaterialCode = "S111-SERVICE-GSF-JH",
                        MaterialDescription = "寄回维修费 20210518",
                        Unit = "PCS",
                        UnitPrice = quotationsMap.ServiceChargeJHCost,
                        SalesPrice = quotationsMap.ServiceChargeJH,
                        Count = quotationsMap.ServiceChargeManHourJH != null ? Convert.ToDecimal(quotationsMap.ServiceChargeManHourJH) / count : 1,
                        TotalPrice = quotationsMap.ServiceChargeManHourJH != null ? (Convert.ToDecimal(quotationsMap.ServiceChargeManHourJH) / count) * quotationsMap.ServiceChargeJH : quotationsMap.ServiceChargeJH / count,
                        Discount = 100,
                        DiscountPrices = quotationsMap.ServiceChargeJH,
                        QuotationMaterialPictures = new List<QuotationMaterialPictureReq>(),
                        MaterialType = quotationsMap.IsMaterialType == "3" ? "4" : "2",
                        Commission = quotationsMap.ServiceChargeJHTC
                    });
                }
                if ((quotationsMap.ServiceChargeSM != null && quotationsMap.ServiceChargeSM > 0) || (quotationsMap.ServiceChargeSMCost != null && quotationsMap.ServiceChargeSMCost > 0))
                {
                    QuotationMergeMaterial.Add(new QuotationMaterialReq
                    {
                        Id = Guid.NewGuid().ToString(),
                        MaterialCode = "S111-SERVICE-GSF-SM",
                        MaterialDescription = "上门维修费 20210518",
                        Unit = "PCS",
                        UnitPrice = quotationsMap.ServiceChargeSMCost,
                        SalesPrice = quotationsMap.ServiceChargeSM,
                        Count = quotationsMap.ServiceChargeManHourSM != null ? Convert.ToDecimal(quotationsMap.ServiceChargeManHourSM) / count : 1,
                        TotalPrice = quotationsMap.ServiceChargeManHourSM != null ? (Convert.ToDecimal(quotationsMap.ServiceChargeManHourSM) / count) * quotationsMap.ServiceChargeSM : quotationsMap.ServiceChargeSM / count,
                        Discount = 100,
                        QuotationMaterialPictures = new List<QuotationMaterialPictureReq>(),
                        DiscountPrices = quotationsMap.ServiceChargeSM,
                        MaterialType = quotationsMap.IsMaterialType == "3" ? "4" : "2",
                        Commission = quotationsMap.ServiceChargeSMTC
                    });
                }
                if ((quotationsMap.TravelExpense != null && quotationsMap.TravelExpense > 0) || (quotationsMap.TravelExpenseCost != null && quotationsMap.TravelExpenseCost > 0))
                {
                    QuotationMergeMaterial.Add(new QuotationMaterialReq
                    {
                        Id = Guid.NewGuid().ToString(),
                        MaterialCode = "S111-SERVICE-CLF",
                        MaterialDescription = "差旅费",
                        Unit = "PCS",
                        UnitPrice = quotationsMap.TravelExpenseCost,
                        SalesPrice = quotationsMap.TravelExpense,
                        Count = quotationsMap.TravelExpenseManHour != null ? Convert.ToDecimal(quotationsMap.TravelExpenseManHour) / count : 1,
                        TotalPrice = quotationsMap.TravelExpenseManHour != null ? (Convert.ToDecimal(quotationsMap.TravelExpenseManHour) / count) * quotationsMap.TravelExpense : quotationsMap.TravelExpense / count,
                        Discount = 100,
                        QuotationMaterialPictures = new List<QuotationMaterialPictureReq>(),
                        DiscountPrices = quotationsMap.TravelExpense,
                        MaterialType = quotationsMap.IsMaterialType == "3" ? "4" : "2",
                        Commission = quotationsMap.TravelExpenseTC
                    });

                }

            }

            quotationsMap.QuotationProducts.ForEach(q =>
            {
                q.FromTheme = serialNumberList.Where(s => s.ManufacturerSerialNumber.Equals(q.ProductCode)).FirstOrDefault()?.FromTheme;
                q.QuotationMaterials.AddRange(QuotationMergeMaterial.ToList());
                q.QuotationMaterials = q.QuotationMaterials.OrderBy(m => m.MaterialCode).ToList();
            });
            quotationsMap.QuotationOperationHistorys = quotationsMap.QuotationOperationHistorys.Where(q => q.ApprovalStage != "-1").OrderBy(o => o.CreateTime).ThenByDescending(o => o.Action).ToList();
            //var operationHistorys = Quotations.QuotationOperationHistorys.Select(q => new OperationHistoryResp { ApprovalResult = q.ApprovalResult, ApprovalStage = q.ApprovalStage, Content = q.Action, CreateTime = q.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"), Remark = q.Remark, CreateUserName = q.CreateUser, IntervalTime = q.IntervalTime.ToString() }).ToList();
            if (!string.IsNullOrWhiteSpace(Quotations.FlowInstanceId) && Quotations.CreateTime>DateTime.Parse("2021-08-12"))
            {
                quotationsMap.FlowPathResp = await _flowInstanceApp.FlowPathRespList(null, Quotations.FlowInstanceId);
            }
            return quotationsMap;
        }

        /// <summary>
        /// 查询报价单详情物料
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> GetDetailsMaterial(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var Quotations = await UnitWork.Find<Quotation>(q => q.Id.Equals(request.QuotationId)).Include(q => q.QuotationPictures).Include(q => q.QuotationProducts).ThenInclude(p => p.QuotationMaterials).FirstOrDefaultAsync();
            var result = new TableData();
            if (!string.IsNullOrWhiteSpace(request.MaterialCode))
            {
                Quotations.QuotationProducts = Quotations.QuotationProducts.Where(q => q.MaterialCode.Contains(request.MaterialCode)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(request.ManufacturerSerialNumbers))
            {
                Quotations.QuotationProducts = Quotations.QuotationProducts.Where(q => q.ProductCode.Contains(request.ManufacturerSerialNumbers)).ToList();
            }
            result.Data = Quotations.QuotationProducts;

            return result;
        }

        /// <summary>
        /// 按条件查询所有物料
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> MaterialCodeList(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //限制查不出bom的数据，暂时屏蔽
            //var equipmentList = await EquipmentList(request);
            //var codeList = equipmentList.Select(e => e.ItemCode).ToList();
            var result = new TableData();
            var query = from a in UnitWork.Find<OITM>(null).WhereIf(!string.IsNullOrWhiteSpace(request.PartCode), q => q.ItemCode.Contains(request.PartCode))
                                .WhereIf(!string.IsNullOrWhiteSpace(request.PartDescribe), q => q.ItemName.Contains(request.PartDescribe))
                                .WhereIf(!string.IsNullOrWhiteSpace(request.ReplacePartCode), q => !q.ItemCode.Equals(request.ReplacePartCode))
                                .WhereIf(!string.IsNullOrWhiteSpace(request.AppPartCode), s => s.ItemName.Contains(request.AppPartCode) || s.ItemCode.Contains(request.AppPartCode))
                            //.WhereIf(codeList.Count > 0, q => !codeList.Contains(q.ItemCode))
                        join b in UnitWork.Find<OITW>(null) on a.ItemCode equals b.ItemCode into ab
                        from b in ab.DefaultIfEmpty()
                        where b.WhsCode == request.WhsCode
                        select new SysEquipmentColumn { ItemCode = a.ItemCode, ItemName = a.ItemName, lastPurPrc = a.LastPurPrc, BuyUnitMsr = a.SalUnitMsr, OnHand = b.OnHand, WhsCode = b.WhsCode };
            //退料获取可替换物料编码
            if (!string.IsNullOrWhiteSpace(request.ItemCode))
            {
                var code = request.ItemCode.Substring(0, request.ItemCode.IndexOf("-") + 1);
                query = query.Where(q => q.ItemCode.Substring(0, q.ItemCode.IndexOf("-") + 1).Equals(code));
            }
            if (request.QueryType == 1)
            {
                //A6开头 带机柜、机箱的物料不可见
                var filterCode = await UnitWork.Find<OITM>(c => c.ItemCode.StartsWith("A604") && (c.ItemName.StartsWith("机柜") || c.ItemName.StartsWith("机箱"))).Select(c => c.ItemCode).ToListAsync();
                query = query.Where(c => !filterCode.Contains(c.ItemCode));
            }

            //是否延保
            if (request.IsWarranty != null && (bool)request.IsWarranty)
            {
                query = query.Where(e => e.ItemCode.Equals("S111-SERVICE-YB"));
            }
            else
            {
                var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ShieldingMaterials")).Select(u => u.Name).ToListAsync();
                query = query.Where(e => !CategoryList.Contains(e.ItemCode));
            }

            if (request.IsCommonUsed != null && (bool)request.IsCommonUsed)
            {
                var obj = await UnitWork.Find<CommonUsedMaterial>(null).Select(c => c.MaterialCode).ToListAsync();
                query = query.Where(e => !obj.Contains(e.ItemCode));
            }
            result.Count = await query.CountAsync();
            var Equipments = await query.Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();
            var ItemCodes = Equipments.Select(e => e.ItemCode).ToList();
            var MaterialPrices = await UnitWork.Find<MaterialPrice>(m => ItemCodes.Contains(m.MaterialCode)).ToListAsync();
            Equipments.ForEach(e =>
            {
                var Prices = MaterialPrices.Where(m => m.MaterialCode.Equals(e.ItemCode)).FirstOrDefault();
                //4.0存在物料价格，取4.0的价格为售后结算价，不存在就当前进货价*1.2 为售后结算价。销售价均为售后结算价*3
                if (Prices != null)
                {
                    e.UnitPrice = Prices?.SettlementPrice == null || Prices?.SettlementPrice <= 0 ? e.lastPurPrc * Prices?.SettlementPriceModel : Prices?.SettlementPrice;
                    //var s = e.UnitPrice.ToDouble().ToString();
                    //if (s.IndexOf(".") > 0)
                    //{
                    //    s = s.Substring(s.IndexOf("."), s.Length - s.IndexOf("."));
                    //    if (s.Length > 1)
                    //    {
                    //        int lengeth = s.Substring(1, s.Length - 1).Length;
                    //        if (lengeth > 3) e.UnitPrice = e.UnitPrice + 0.005M;
                    //    }
                    //}
                    e.UnitPrice = decimal.Parse(e.UnitPrice.ToString("#0.0000"));
                    e.lastPurPrc = e.UnitPrice * Prices?.SalesMultiple;
                }
                else
                {
                    e.UnitPrice = e.lastPurPrc * 1.2M;
                    //var s = e.UnitPrice.ToDouble().ToString();
                    //if (s.IndexOf(".") > 0)
                    //{
                    //    s = s.Substring(s.IndexOf("."), s.Length - s.IndexOf("."));
                    //    if (s.Length > 1)
                    //    {
                    //        int lengeth = s.Substring(1, s.Length - 1).Length;
                    //        if (lengeth > 3) e.UnitPrice = e.UnitPrice + 0.005M;
                    //    }
                    //}
                    e.UnitPrice = decimal.Parse(e.UnitPrice.ToString("#0.0000"));
                    e.lastPurPrc = e.UnitPrice * 3;
                    //A6开头 不带机箱、机柜的物料 销售价为结算价的2倍
                    if (e.ItemCode.StartsWith("A6") && !(e.ItemName.Contains("机柜") || e.ItemName.Contains("机箱")))
                        e.lastPurPrc = e.UnitPrice * 2;
                    if (e.ItemCode == "A801-7'S-CC-WX")
                        e.lastPurPrc = e.UnitPrice * 2;
                }

            });
            result.Data = Equipments.ToList();
            return result;
        }

        #region 常用物料
        /// <summary>
        /// 添加常用物料
        /// </summary>
        /// <returns></returns>
        public async Task AddCommonUsedMaterial(List<AddCommonUsedMaterialReq>  request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var obj = request.MapToList<CommonUsedMaterial>();
            var materialCode = obj.Select(c => c.MaterialCode).ToList();
            var material = await UnitWork.Find<CommonUsedMaterial>(c => materialCode.Contains(c.MaterialCode)).FirstOrDefaultAsync();
            if (material!=null)
            {
                throw new CommonException($"物料{material.MaterialCode}已设为常用物料，勿重复操作。", 500);
            }
            await UnitWork.BatchAddAsync(obj.ToArray());
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 删除常用物料
        /// </summary>
        /// <param name="materialCode"></param>
        /// <returns></returns>
        public async Task DeleteCommonUsedMaterial(string materialCode)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var obj = await UnitWork.Find<CommonUsedMaterial>(c => c.MaterialCode == materialCode).FirstOrDefaultAsync();
            await UnitWork.DeleteAsync(obj);
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 获取常用物料
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetCommonUsedMaterial(QueryQuotationListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();
            var obj = await UnitWork.Find<CommonUsedMaterial>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(req.PartCode), q => q.MaterialCode.Contains(req.PartCode))
                .ToListAsync();
            result.Data = obj.Skip((req.page - 1) * req.limit).Take(req.limit).ToList();
            result.Count = obj.Count;
            return result;
        }
        #endregion

        /// <summary>
        /// 获取待合并报价单
        /// </summary>
        /// <param name="ServiceOrderId"></param>
        /// <returns></returns>
        public async Task<TableData> GetUnreadQuotations(int ServiceOrderId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var Quotations = await UnitWork.Find<Quotation>(q => q.ServiceOrderId.Equals(ServiceOrderId) && q.ErpOrApp == 2).ToListAsync();
            var QuotationIds = Quotations.Select(q => q.Id).ToList();
            var QuotationProducts = await UnitWork.Find<QuotationProduct>(q => QuotationIds.Contains((int)q.QuotationId)).Include(q => q.QuotationMaterials).ToListAsync();
            Quotations.ForEach(q => q.QuotationProducts = QuotationProducts.Where(p => p.QuotationId.Equals(q.Id)).ToList());
            result.Data = new
            {
                Quotations,
            };

            return result;
        }

        /// <summary>
        /// 获取该服务单所有报价单零件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> GetQuotationMaterialCode(QueryQuotationListReq request)
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
            var result = new TableData();
            //var QuotationIds = await UnitWork.Find<Quotation>(q => q.ServiceOrderId.Equals(request.ServiceOrderId) && q.CreateUserId.Equals(loginUser.Id)).Select(q => q.Id).ToListAsync();

            //var QuotationMergeMaterials = await UnitWork.Find<QuotationMergeMaterial>(q => QuotationIds.Contains((int)q.QuotationId) && q.MaterialType == 1).ToListAsync();
            ////获取当前服务单所有退料明细汇总
            //var query = from a in UnitWork.Find<ReturnNoteMaterial>(null)
            //            join b in UnitWork.Find<ReturnNote>(null) on a.ReturnNoteId equals b.Id into ab
            //            from b in ab.DefaultIfEmpty()
            //            where b.ServiceOrderId == request.ServiceOrderId
            //            select new { a.Id };
            //var returnMaterials = (await query.ToListAsync()).GroupBy(g => g.QuotationMaterialId).Select(s => new { Qty = s.Sum(s => s.Count), Id = s.Key }).ToList();
            //List<ReturnMaterialListResp> data = new List<ReturnMaterialListResp>();
            //foreach (var item in QuotationMergeMaterials)
            //{
            //    var res = item.MapTo<ReturnMaterialListResp>();
            //    int everQty = (int)(returnMaterials.Where(w => w.Id == item.Id).FirstOrDefault() == null ? 0 : returnMaterials.Where(w => w.Id == item.Id).FirstOrDefault()?.Qty);
            //    res.SurplusQty = (int)item.Count - (returnMaterials.Where(w => w.Id == item.Id).FirstOrDefault() == null ? 0 : (int)returnMaterials.Where(w => w.Id == item.Id).FirstOrDefault().Qty);
            //    data.Add(res);
            //}
            //result.Data = data;
            return result;
        }

        /// <summary>
        /// 新增报价单
        /// </summary>
        /// <param name="obj"></param>
        public async Task<string> Add(AddOrUpdateQuotationReq obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(obj.AppId));
            }
            var Message = await Condition(obj);
            var QuotationObj = await CalculatePrice(obj);
            var dbContext = UnitWork.GetDbContext<Quotation>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    QuotationObj.QuotationProducts.ForEach(q => q.QuotationMaterials.ForEach(m =>
                    {
                        m.Id = Guid.NewGuid().ToString();
                        if (m.QuotationMaterialPictures != null && m.QuotationMaterialPictures.Count() > 0)
                        {
                            m.QuotationMaterialPictures.ForEach(p => p.Id = Guid.NewGuid().ToString());
                        }
                    }));
                    QuotationObj.CreateTime = DateTime.Now;
                    QuotationObj.CreateUser = obj.IsOutsourc != null && (bool)obj.IsOutsourc? obj .CreateUser: loginUser.Name;
                    QuotationObj.CreateUserId = obj.IsOutsourc != null && (bool)obj.IsOutsourc ? obj.CreateUserId : loginUser.Id;
                    QuotationObj.Status = 1;
                    QuotationObj.QuotationStatus = 3;
                    QuotationObj.PrintWarehouse = 1;
                    QuotationObj.UpDateTime = DateTime.Now;
                    QuotationObj = await UnitWork.AddAsync<Quotation, int>(QuotationObj);
                    await UnitWork.SaveAsync();
                    if (!obj.IsDraft)
                    {
                        QuotationObj.QuotationStatus = 3.1M;
                        await MergeMaterial(QuotationObj);
                        QuotationOperationHistory quotationOperationHistory = new QuotationOperationHistory
                        {
                            Action = QuotationObj.ErpOrApp == 1 ? QuotationObj.CreateUser + "通过ERP提交审批" : QuotationObj.CreateUser + "通过APP提交审批",
                            CreateUser = loginUser.Name,
                            CreateUserId = loginUser.Id,
                            CreateTime = DateTime.Now,
                            QuotationId = QuotationObj.Id,
                            ApprovalStage = "3"
                        };
                        if (obj.IsOutsourc != null && (bool)obj.IsOutsourc)
                        {
                            QuotationObj.QuotationStatus = 10;
                            QuotationObj.Status = 2;
                            await UnitWork.UpdateAsync<Quotation>(QuotationObj);
                            await UnitWork.SaveAsync();
                            quotationOperationHistory.Action = "个代结算系统自动提交";
                            #region 报价单同步到SAP，ERP3.0
                            //await _capBus.PublishAsync("Serve.SellOrder.Create", QuotationObj.Id);
                            #endregion
                            Message = QuotationObj.Id.ToString();
                        }
                        else 
                        {

                            #region 创建审批流程
                            var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name.Equals("物料报价单"));
                            var afir = new AddFlowInstanceReq();
                            afir.SchemeId = mf.FlowSchemeId;
                            afir.FrmType = 2;
                            afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();

                            var IsProtected = QuotationObj.IsProtected != null && QuotationObj.IsProtected == true ? "1" : "2";
                            string IsWarranty = null;
                            if (QuotationObj.IsMaterialType == 4)
                            {
                                IsWarranty = "1";
                            }
                            else
                            {
                                IsWarranty = "2";
                            }
                            afir.FrmData = "{ \"QuotationId\":\"" + QuotationObj.Id + "\",\"IsProtected\":\"" + IsProtected + "\",\"IsWarranty\":\"" + IsWarranty + "\",\"WarrantyType\":\"" + QuotationObj.WarrantyType + "\"}";
                            afir.CustomName = $"物料报价单" + DateTime.Now;
                            QuotationObj.FlowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(afir);
                            #endregion
                            //增加全局待处理
                            var serviceOrederObj = await UnitWork.Find<ServiceOrder>(s => s.Id == obj.ServiceOrderId).FirstOrDefaultAsync();
                            await _workbenchApp.AddOrUpdate(new WorkbenchPending
                            {
                                OrderType = 1,
                                TerminalCustomer = serviceOrederObj.TerminalCustomer,
                                TerminalCustomerId = serviceOrederObj.TerminalCustomerId,
                                ServiceOrderId = serviceOrederObj.Id,
                                ServiceOrderSapId = (int)serviceOrederObj.U_SAP_ID,
                                UpdateTime = QuotationObj.UpDateTime,
                                Remark = QuotationObj.Remark,
                                FlowInstanceId = QuotationObj.FlowInstanceId,
                                TotalMoney = QuotationObj.TotalMoney,
                                Petitioner = loginUser.Name,
                                SourceNumbers = QuotationObj.Id,
                                PetitionerId = loginUser.Id,
                            });
                        }
                        await UnitWork.AddAsync<QuotationOperationHistory>(quotationOperationHistory);
                        await UnitWork.UpdateAsync<Quotation>(QuotationObj);
                        await UnitWork.SaveAsync();
                    }
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("添加报价单失败。" + ex.Message);
                }
            }
            
            return Message;
        }

        /// <summary>
        /// 修改报价单
        /// </summary>
        /// <param name="obj"></param>
        public async Task<string> Update(AddOrUpdateQuotationReq obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(obj.AppId));
            }
            var Message = await Condition(obj);
            var QuotationObj = await CalculatePrice(obj);
            var dbContext = UnitWork.GetDbContext<Quotation>();
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    #region 删除
                    await UnitWork.DeleteAsync<QuotationProduct>(q => q.QuotationId == QuotationObj.Id);
                    await UnitWork.DeleteAsync<QuotationMergeMaterial>(q => q.QuotationId.Equals(QuotationObj.Id));
                    await UnitWork.SaveAsync();
                    #endregion

                    #region 新增
                    if (QuotationObj.QuotationProducts != null && QuotationObj.QuotationProducts.Count > 0)
                    {
                        var QuotationProductMap = QuotationObj.QuotationProducts.MapToList<QuotationProduct>();
                        QuotationProductMap.ForEach(q =>
                        {
                            q.QuotationMaterials.ForEach(m =>
                            {
                                m.Id = Guid.NewGuid().ToString();
                                if (m.QuotationMaterialPictures != null && m.QuotationMaterialPictures.Count() > 0)
                                {
                                    m.QuotationMaterialPictures.ForEach(p => { p.Id = Guid.NewGuid().ToString(); });
                                }
                            });
                        });
                        await UnitWork.BatchAddAsync<QuotationProduct>(QuotationProductMap.ToArray());
                    }
                    await UnitWork.SaveAsync();

                    #endregion

                    if (obj.IsDraft)
                    {
                        await UnitWork.UpdateAsync<Quotation>(u => u.Id == QuotationObj.Id, u => new Quotation
                        {
                            QuotationStatus = 3,
                            CollectionAddress = QuotationObj.CollectionAddress,
                            ShippingAddress = QuotationObj.ShippingAddress,
                            DeliveryMethod = QuotationObj.DeliveryMethod,
                            InvoiceCompany = QuotationObj.InvoiceCompany,
                            TotalMoney = QuotationObj.TotalMoney,
                            TotalCommission = QuotationObj.TotalCommission,
                            CommissionAmount1 = QuotationObj.CommissionAmount1,
                            CommissionAmount2 = QuotationObj.CommissionAmount2,
                            TotalCostPrice = QuotationObj.TotalCostPrice,
                            Remark = QuotationObj.Remark,
                            IsDraft = QuotationObj.IsDraft,
                            IsProtected = QuotationObj.IsProtected,
                            TravelExpense = QuotationObj.TravelExpense,
                            Status = 1,
                            ServiceChargeJH = QuotationObj.ServiceChargeJH,
                            ServiceChargeSM = QuotationObj.ServiceChargeSM,
                            TaxRate = QuotationObj.TaxRate,
                            InvoiceCategory = QuotationObj.InvoiceCategory,
                            AcceptancePeriod = QuotationObj.AcceptancePeriod,
                            Prepay = QuotationObj.Prepay,
                            PaymentAfterWarranty = QuotationObj.PaymentAfterWarranty,
                            CashBeforeFelivery = QuotationObj.CashBeforeFelivery,
                            PayOnReceipt = QuotationObj.PayOnReceipt,
                            CollectionDA = QuotationObj.CollectionDA,
                            ShippingDA = QuotationObj.ShippingDA,
                            AcquisitionWay = QuotationObj.AcquisitionWay,
                            IsMaterialType = QuotationObj.IsMaterialType,
                            ServiceChargeManHourJH = QuotationObj.ServiceChargeManHourJH,
                            ServiceChargeManHourSM = QuotationObj.ServiceChargeManHourSM,
                            ServiceChargeSMTC = QuotationObj.ServiceChargeSMTC,
                            TravelExpenseTC = QuotationObj.TravelExpenseTC,
                            ServiceChargeJHTC = QuotationObj.ServiceChargeJHTC,
                            TravelExpenseManHour = QuotationObj.TravelExpenseManHour,
                            ServiceChargeJHCost = QuotationObj.ServiceChargeJHCost,
                            ServiceChargeSMCost = QuotationObj.ServiceChargeSMCost,
                            TravelExpenseCost = QuotationObj.TravelExpenseCost,
                            PrintWarehouse = 1,
                            MoneyMeans = QuotationObj.MoneyMeans,
                            UpDateTime = DateTime.Now,
                            NewestContacter = QuotationObj.NewestContacter,
                            NewestContactTel = QuotationObj.NewestContactTel
                            //todo:要修改的字段赋值
                        });
                        await UnitWork.SaveAsync();
                    }
                    else
                    {
                        await MergeMaterial(QuotationObj);
                        var IsProtected = QuotationObj.IsProtected != null && QuotationObj.IsProtected == true ? "1" : "2";
                        string IsWarranty = null;
                        if (QuotationObj.IsMaterialType == 4)
                        {
                            IsWarranty = "1";
                        }
                        else
                        {
                            IsWarranty = "2";
                        }
                        var FrmData = "{ \"QuotationId\":\"" + QuotationObj.Id + "\",\"IsProtected\":\"" + IsProtected + "\",\"IsWarranty\":\"" + IsWarranty + "\",\"WarrantyType\":\"" + QuotationObj.WarrantyType + "\"}";
                        var flowInstanceObj = await UnitWork.Find<FlowInstance>(f => f.Id.Equals(QuotationObj.FlowInstanceId)).FirstOrDefaultAsync();
                        if (string.IsNullOrWhiteSpace(QuotationObj.FlowInstanceId)|| (flowInstanceObj != null && flowInstanceObj.FrmData != FrmData))
                        {
                            #region 创建审批流程
                            var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name.Equals("物料报价单"));
                            var afir = new AddFlowInstanceReq();
                            afir.SchemeId = mf.FlowSchemeId;
                            afir.FrmType = 2;
                            afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                            afir.CustomName = $"物料报价单" + DateTime.Now;
                            afir.FrmData = FrmData;
                            QuotationObj.FlowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(afir);
                            #endregion
                        }
                        else
                        {
                            await _flowInstanceApp.Start(new StartFlowInstanceReq { FlowInstanceId = QuotationObj.FlowInstanceId });
                        }
                        await UnitWork.UpdateAsync<Quotation>(u => u.Id == QuotationObj.Id, u => new Quotation
                        {
                            QuotationStatus = 3.1M,
                            CollectionAddress = QuotationObj.CollectionAddress,
                            ShippingAddress = QuotationObj.ShippingAddress,
                            DeliveryMethod = QuotationObj.DeliveryMethod,
                            InvoiceCompany = QuotationObj.InvoiceCompany,
                            TotalMoney = QuotationObj.TotalMoney,
                            TotalCommission = QuotationObj.TotalCommission,
                            CommissionAmount1 = QuotationObj.CommissionAmount1,
                            CommissionAmount2 = QuotationObj.CommissionAmount2,
                            TotalCostPrice = QuotationObj.TotalCostPrice,
                            Remark = QuotationObj.Remark,
                            IsDraft = QuotationObj.IsDraft,
                            IsProtected = QuotationObj.IsProtected,
                            TravelExpense = QuotationObj.TravelExpense,
                            Status = 1,
                            ServiceChargeJH = QuotationObj.ServiceChargeJH,
                            ServiceChargeSM = QuotationObj.ServiceChargeSM,
                            Prepay = QuotationObj.Prepay,
                            TaxRate = QuotationObj.TaxRate,
                            InvoiceCategory = QuotationObj.InvoiceCategory,
                            AcceptancePeriod = QuotationObj.AcceptancePeriod,
                            PaymentAfterWarranty = QuotationObj.PaymentAfterWarranty,
                            CashBeforeFelivery = QuotationObj.CashBeforeFelivery,
                            PayOnReceipt = QuotationObj.PayOnReceipt,
                            CollectionDA = QuotationObj.CollectionDA,
                            ShippingDA = QuotationObj.ShippingDA,
                            AcquisitionWay = QuotationObj.AcquisitionWay,
                            IsMaterialType = QuotationObj.IsMaterialType,
                            ServiceChargeManHourJH = QuotationObj.ServiceChargeManHourJH,
                            ServiceChargeManHourSM = QuotationObj.ServiceChargeManHourSM,
                            ServiceChargeSMTC = QuotationObj.ServiceChargeSMTC,
                            TravelExpenseTC = QuotationObj.TravelExpenseTC,
                            ServiceChargeJHTC = QuotationObj.ServiceChargeJHTC,
                            TravelExpenseManHour = QuotationObj.TravelExpenseManHour,
                            ServiceChargeJHCost = QuotationObj.ServiceChargeJHCost,
                            ServiceChargeSMCost = QuotationObj.ServiceChargeSMCost,
                            TravelExpenseCost = QuotationObj.TravelExpenseCost,
                            PrintWarehouse = 1,
                            MoneyMeans = QuotationObj.MoneyMeans,
                            UpDateTime = DateTime.Now,
                            FlowInstanceId = QuotationObj.FlowInstanceId,
                            NewestContacter = QuotationObj.NewestContacter,
                            NewestContactTel = QuotationObj.NewestContactTel
                            //todo:要修改的字段赋值
                        });
                        await UnitWork.AddAsync<QuotationOperationHistory>(new QuotationOperationHistory
                        {
                            Action = QuotationObj.ErpOrApp == 1 ? QuotationObj.CreateUser + "通过ERP提交审批" : QuotationObj.CreateUser + "通过APP提交审批",
                            CreateUser = loginUser.Name,
                            CreateUserId = loginUser.Id,
                            CreateTime = DateTime.Now,
                            QuotationId = QuotationObj.Id,
                            ApprovalStage = "3"
                        });
                        //增加全局待处理
                        var serviceOrederObj = await UnitWork.Find<ServiceOrder>(s => s.Id == obj.ServiceOrderId).FirstOrDefaultAsync();
                        await _workbenchApp.AddOrUpdate(new WorkbenchPending
                        {
                            OrderType = 1,
                            TerminalCustomer = serviceOrederObj.TerminalCustomer,
                            TerminalCustomerId = serviceOrederObj.TerminalCustomerId,
                            ServiceOrderId = serviceOrederObj.Id,
                            ServiceOrderSapId = (int)serviceOrederObj.U_SAP_ID,
                            UpdateTime = DateTime.Now,
                            Remark = QuotationObj.Remark,
                            FlowInstanceId = QuotationObj.FlowInstanceId,
                            TotalMoney = QuotationObj.TotalMoney,
                            Petitioner = loginUser.Name,
                            SourceNumbers = QuotationObj.Id,
                            PetitionerId = loginUser.Id,
                        });
                        await UnitWork.SaveAsync();

                    }
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("添加报价单失败,请重试。" + ex.Message);
                }
                return Message;
            }
        }

        /// <summary>
        /// 合并零件表
        /// </summary>
        /// <param name="QuotationObj"></param>
        /// <returns></returns>
        public async Task MergeMaterial(Quotation QuotationObj)
        {
            #region 合并零件表
            List<QuotationMaterial> QuotationMaterials = new List<QuotationMaterial>();
            QuotationObj.QuotationProducts.ToList().ForEach(q => QuotationMaterials.AddRange(q.QuotationMaterials));

            //var MaterialsT = from a in QuotationMaterials
            //                 group a by new { a.MaterialCode, a.MaterialDescription, a.Unit, a.SalesPrice, a.UnitPrice, a.Discount, a.MaterialType, a.DiscountPrices, a.WhsCode } into g
            //                 select new QueryQuotationMergeMaterialListReq
            //                 {
            //                     MaterialCode = g.Key.MaterialCode,
            //                     MaterialDescription = g.Key.MaterialDescription,
            //                     Unit = g.Key.Unit,
            //                     SalesPrice = g.Key.SalesPrice,
            //                     CostPrice = g.Key.UnitPrice,
            //                     Count = g.Sum(a => a.Count),
            //                     TotalPrice = (g.Key.DiscountPrices * g.Sum(a => a.Count)),
            //                     IsProtected = QuotationObj.IsMaterialType==2? false : true,
            //                     QuotationId = QuotationObj.Id,
            //                     Margin = (g.Key.DiscountPrices * g.Sum(a => a.Count)) - (g.Key.UnitPrice * g.Sum(a => a.Count)),
            //                     Discount = g.Key.Discount,
            //                     SentQuantity = 0,
            //                     MaterialType = (int)g.Key.MaterialType,
            //                     DiscountPrices = g.Key.DiscountPrices,
            //                     WhsCode = g.Key.WhsCode
            //                 };
            var quotationMaterialsList = QuotationMaterials.GroupBy(q => new { q.MaterialCode, q.Unit, q.MaterialType, q.DiscountPrices, q.WhsCode }).Select(q => new { mergeMaterial = q.First(), count = q.Sum(s => s.Count) }).ToList();
            var MaterialsT = quotationMaterialsList.Select(q => new QueryQuotationMergeMaterialListReq
            {
                MaterialCode = q.mergeMaterial.MaterialCode,
                MaterialDescription = q.mergeMaterial.MaterialDescription,
                Unit = q.mergeMaterial.Unit,
                SalesPrice = q.mergeMaterial.SalesPrice,
                CostPrice = q.mergeMaterial.UnitPrice,
                Count = q.count,
                TotalPrice = (q.mergeMaterial.DiscountPrices * q.count),
                IsProtected = QuotationObj.IsMaterialType == 2 ? false : true,
                QuotationId = QuotationObj.Id,
                Margin = (q.mergeMaterial.DiscountPrices * q.count) - (q.mergeMaterial.UnitPrice * q.count),
                Discount = q.mergeMaterial.Discount,
                SentQuantity = 0,
                MaterialType = (int)q.mergeMaterial.MaterialType,
                DiscountPrices = q.mergeMaterial.DiscountPrices,
                WhsCode = q.mergeMaterial.WhsCode,
                Commission = q.mergeMaterial.Commission
            });
            var QuotationMergeMaterialList = MaterialsT.ToList();

            if ((QuotationObj.ServiceChargeJH != null && QuotationObj.ServiceChargeJH > 0) || (QuotationObj.ServiceChargeJHCost != null && QuotationObj.ServiceChargeJHCost > 0))
            {
                QuotationMergeMaterialList.Add(new QueryQuotationMergeMaterialListReq
                {
                    MaterialCode = "S111-SERVICE-GSF-JH",
                    MaterialDescription = "寄回维修费 20210518",
                    Unit = "PCS",
                    SalesPrice = QuotationObj.ServiceChargeJH,
                    CostPrice = QuotationObj.ServiceChargeJHCost,
                    Count = QuotationObj.ServiceChargeManHourJH,
                    TotalPrice = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeJH * QuotationObj.ServiceChargeManHourJH).ToString("#0.00")),
                    IsProtected = QuotationObj.IsMaterialType == 2 ? false : true,
                    QuotationId = QuotationObj.Id,
                    Margin = QuotationObj.ServiceChargeJH != null && QuotationObj.ServiceChargeJH > 0 ? QuotationObj.ServiceChargeJH * QuotationObj.ServiceChargeManHourJH : -(QuotationObj.ServiceChargeManHourJH * QuotationObj.ServiceChargeJHCost),
                    Discount = 100,
                    SentQuantity = 0,
                    MaterialType = QuotationObj.IsMaterialType == 3 ? 4 : 2,
                    DiscountPrices = QuotationObj.ServiceChargeJH,
                    WhsCode = "37",
                    Commission = QuotationObj.IsMaterialType == 2 ? QuotationObj.ServiceChargeJHTC : 0
                });
            }
            if ((QuotationObj.ServiceChargeSM != null && QuotationObj.ServiceChargeSM > 0) || (QuotationObj.ServiceChargeSMCost != null && QuotationObj.ServiceChargeSMCost > 0))
            {
                QuotationMergeMaterialList.Add(new QueryQuotationMergeMaterialListReq
                {
                    MaterialCode = "S111-SERVICE-GSF-SM",
                    MaterialDescription = "上门维修费 20210518",
                    Unit = "PCS",
                    SalesPrice = QuotationObj.ServiceChargeSM,
                    CostPrice = QuotationObj.ServiceChargeSMCost,
                    Count = QuotationObj.ServiceChargeManHourSM,
                    TotalPrice = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeSM * QuotationObj.ServiceChargeManHourSM).ToString("#0.00")),
                    IsProtected = QuotationObj.IsMaterialType == 2 ? false : true,
                    QuotationId = QuotationObj.Id,
                    Margin = QuotationObj.ServiceChargeSM != null && QuotationObj.ServiceChargeSM > 0 ? QuotationObj.ServiceChargeSM * QuotationObj.ServiceChargeManHourSM : -(QuotationObj.ServiceChargeManHourSM * QuotationObj.ServiceChargeSMCost),
                    Discount = 100,
                    SentQuantity = 0,
                    MaterialType = QuotationObj.IsMaterialType==3?4:2,
                    DiscountPrices = QuotationObj.ServiceChargeSM,
                    WhsCode = "37",
                    Commission = QuotationObj.IsMaterialType == 2 ? QuotationObj.ServiceChargeSMTC : 0
                });
            }
            if ((QuotationObj.TravelExpense != null && QuotationObj.TravelExpense > 0) || (QuotationObj.TravelExpenseCost != null && QuotationObj.TravelExpenseCost > 0))
            {
                QuotationMergeMaterialList.Add(new QueryQuotationMergeMaterialListReq
                {
                    MaterialCode = "S111-SERVICE-CLF",
                    MaterialDescription = "差旅费",
                    Unit = "PCS",
                    SalesPrice = QuotationObj.TravelExpense,
                    CostPrice = QuotationObj.TravelExpenseCost,
                    Count = QuotationObj.TravelExpenseManHour,
                    TotalPrice = Convert.ToDecimal(Convert.ToDecimal(QuotationObj.TravelExpense * QuotationObj.TravelExpenseManHour).ToString("#0.00")),
                    IsProtected = QuotationObj.IsMaterialType == 2 ? false : true,
                    QuotationId = QuotationObj.Id,
                    Margin = QuotationObj.TravelExpense != null && QuotationObj.TravelExpense > 0 ? QuotationObj.TravelExpense * QuotationObj.TravelExpenseManHour : -(QuotationObj.TravelExpenseManHour * QuotationObj.TravelExpenseCost),
                    Discount = 100,
                    SentQuantity = 0,
                    MaterialType = QuotationObj.IsMaterialType == 3 ? 4 : 2,
                    DiscountPrices = QuotationObj.TravelExpense,
                    WhsCode = "37",
                    Commission = QuotationObj.IsMaterialType == 2 ? QuotationObj.TravelExpenseTC : 0
                });
            }
            var QuotationMergeMaterialListMap = QuotationMergeMaterialList.MapToList<QuotationMergeMaterial>();
            await UnitWork.BatchAddAsync<QuotationMergeMaterial>(QuotationMergeMaterialListMap.ToArray());
            await UnitWork.SaveAsync();
            #endregion
        }

        /// <summary>
        /// 撤回报价单
        /// </summary>
        /// <param name="QuotationId"></param>
        public async Task Revocation(int QuotationId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var quotationObj = await UnitWork.Find<Quotation>(q => q.Id == QuotationId && q.QuotationStatus <= 5).FirstOrDefaultAsync();
            if (quotationObj == null)
            {
                throw new Exception("该报价单状态不可撤销。");
            }
            await UnitWork.UpdateAsync<Quotation>(q => q.Id == QuotationId, q => new Quotation
            {
                QuotationStatus = 2
            });
            var delQuotationMergeMaterial = await UnitWork.Find<QuotationMergeMaterial>(q => q.QuotationId.Equals(QuotationId)).ToListAsync();
            await UnitWork.BatchDeleteAsync<QuotationMergeMaterial>(delQuotationMergeMaterial.ToArray());
            var selqoh = await UnitWork.Find<QuotationOperationHistory>(r => r.QuotationId.Equals(QuotationId)).OrderByDescending(r => r.CreateTime).FirstOrDefaultAsync();
            QuotationOperationHistory qoh = new QuotationOperationHistory();
            qoh.CreateUser = loginContext.User.Name;
            qoh.CreateUserId = loginContext.User.Id;
            qoh.CreateTime = DateTime.Now;
            qoh.QuotationId = QuotationId;
            qoh.ApprovalResult = "撤回";
            qoh.Action = "撤回报价单";
            qoh.ApprovalStage = "2";
            qoh.IntervalTime = selqoh != null ? Convert.ToInt32((DateTime.Now - Convert.ToDateTime(selqoh.CreateTime)).TotalSeconds) : 0;
            await UnitWork.AddAsync<QuotationOperationHistory>(qoh);
            if (!string.IsNullOrWhiteSpace(quotationObj.FlowInstanceId))
            {
                await _flowInstanceApp.ReCall(new RecallFlowInstanceReq { FlowInstanceId = quotationObj.FlowInstanceId });
            }
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 修改报价单物料，物流
        /// </summary>
        /// <param name="obj"></param>
        public async Task<TableData> UpdateMaterial(AddOrUpdateQuotationReq obj)
        {
            var expressageobj = new Expressage();
            var expressageMap = obj.ExpressageReqs.MapTo<Expressage>();
            var loginUser = new User();
            if (expressageMap.ExpressNumber == "自动出库")
            {
                loginUser = await UnitWork.Find<User>(u => u.Account.Equals("Admin")).FirstOrDefaultAsync();
            }
            else
            {
                var loginContext = _auth.GetCurrentUser();
                if (loginContext == null)
                {
                    throw new CommonException("登录已过期", Define.INVALID_TOKEN);
                }
                loginUser = loginContext.User;
                if (!loginContext.Roles.Any(r => r.Name.Equals("仓库")))
                {
                    throw new Exception("无仓库人员权限，不可出库。");
                }
            }
            #region 判断条件
            var quotationObj = await UnitWork.Find<Quotation>(q => q.Id == expressageMap.QuotationId).Include(q => q.QuotationMergeMaterials).FirstOrDefaultAsync();
            var mergeMaterialList = quotationObj.QuotationMergeMaterials.Select(q => new { q.MaterialCode, q.Id, q.WhsCode }).ToList();
            if (quotationObj.SalesOrderId == null || quotationObj.SalesOrderId <= 0)
            {
                throw new Exception("暂未生成销售订单，不可出库，请联系管理员。");
            }
            if(quotationObj.CancelRequest!=null) throw new Exception("已申请取消，不可出库");
            //判定是否存在成品
            mergeMaterialList.ForEach(m =>
            {
                if (m.MaterialCode.Trim().Substring(0, 1) == "C")
                {
                    throw new Exception("本出库单存在成品物料，请到ERP3.0进行交货操作。");
                }
            });
            string message = null;
            //判定库存数量
            var mergeMaterialIds = obj.QuotationMergeMaterialReqs.Select(q => q.Id).ToList();
            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ShieldingMaterials")).Select(u => u.Name).ToListAsync();
            mergeMaterialList = mergeMaterialList.Where(q => mergeMaterialIds.Contains(q.Id) && !CategoryList.Contains(q.MaterialCode)).ToList();
            var mergeMaterials = mergeMaterialList.Select(m => m.MaterialCode).ToList();
            var whscodes = mergeMaterialList.Select(m => m.WhsCode).Distinct();
            var onHand = await UnitWork.Find<OITW>(o => mergeMaterials.Contains(o.ItemCode) && whscodes.Contains(o.WhsCode)).Select(o => new { o.ItemCode, o.OnHand, o.WhsCode }).ToListAsync();
            onHand.ForEach(o =>
            {
                var mergeMaterialid = mergeMaterialList.Where(m => m.MaterialCode.Equals(o.ItemCode) && m.WhsCode.Equals(o.WhsCode)).FirstOrDefault()?.Id;
                var num = obj.QuotationMergeMaterialReqs.Where(q => q.Id == mergeMaterialid).FirstOrDefault()?.SentQuantity;
                if (num != null && num > o.OnHand)
                {
                    message += o.ItemCode + "  ";
                }
            }
             );
            if (!string.IsNullOrWhiteSpace(message))
            {
                throw new Exception(message + "数量降为负库存，不可交货");
            }
            #endregion
            var result = new TableData();
            var dbContext = UnitWork.GetDbContext<Quotation>();
            List<LogisticsRecord> LogisticsRecords = new List<LogisticsRecord>();
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    //用信号量代替锁
                    await semaphoreSlim.WaitAsync();
                    try
                    {
                        if (string.IsNullOrWhiteSpace(expressageMap.ExpressNumber))
                        {
                            var time = DateTime.Now;
                            expressageMap.ExpressNumber = "ZT" + time.Year.ToString() + time.Month.ToString() + time.Day.ToString() + time.Hour.ToString() + time.Minute.ToString() + time.Second.ToString();
                        }
                    }
                    finally
                    {
                        semaphoreSlim.Release();
                    }
                    expressageMap.CreateTime = DateTime.Now;
                    expressageobj = await UnitWork.AddAsync<Expressage>(expressageMap);
                    var ExpressagePictures = new List<ExpressagePicture>();
                    obj.ExpressageReqs.ExpressagePictures.ForEach(p => ExpressagePictures.Add(new ExpressagePicture { ExpressageId = expressageobj.Id, PictureId = p, Id = Guid.NewGuid().ToString() }));
                    await UnitWork.BatchAddAsync<ExpressagePicture>(ExpressagePictures.ToArray());
                    foreach (var item in obj.QuotationMergeMaterialReqs)
                    {
                        LogisticsRecords.Add(new LogisticsRecord
                        {
                            CreateTime = DateTime.Now,
                            CreateUser = loginUser.Name,
                            CreateUserId = loginUser.Id,
                            Quantity = item.SentQuantity,
                            QuotationId = item.QuotationId,
                            QuotationMaterialId = item.Id,
                            ExpressageId = expressageobj.Id
                        });
                        if (item.SentQuantity > 0)
                        {
                            var QuotationMergeMaterialobj = await UnitWork.Find<QuotationMergeMaterial>(q => q.Id.Equals(item.Id)).FirstOrDefaultAsync();
                            QuotationMergeMaterialobj.SentQuantity += item.SentQuantity;
                            await UnitWork.UpdateAsync<QuotationMergeMaterial>(QuotationMergeMaterialobj);
                        }
                    }
                    await UnitWork.BatchAddAsync<LogisticsRecord>(LogisticsRecords.ToArray());
                    await UnitWork.SaveAsync();
                    var Expressages = await UnitWork.Find<Expressage>(e => e.QuotationId.Equals(obj.ExpressageReqs.QuotationId)).Include(e => e.ExpressagePicture).Include(e => e.LogisticsRecords).OrderByDescending(e => e.CreateTime).ToListAsync();
                    LogisticsRecords = new List<LogisticsRecord>();
                    Expressages.ForEach(e => LogisticsRecords.AddRange(e.LogisticsRecords));
                    var QuotationMergeMaterialLists = await UnitWork.Find<QuotationMergeMaterial>(q => q.QuotationId.Equals(obj.ExpressageReqs.QuotationId)).ToListAsync();
                    #region 写入sap和反写记录


                    int isEXwarehouse = QuotationMergeMaterialLists.Where(q => q.SentQuantity != q.Count).Count();
                    List<QuotationOperationHistory> qoh = new List<QuotationOperationHistory>();
                    var selqoh = await UnitWork.Find<QuotationOperationHistory>(r => r.QuotationId.Equals(obj.ExpressageReqs.QuotationId)).OrderByDescending(r => r.CreateTime).FirstOrDefaultAsync();

                    if (selqoh.ApprovalStage != "12")
                    {
                        qoh.Add(new QuotationOperationHistory
                        {
                            Id = Guid.NewGuid().ToString(),
                            Action = "开始出库",
                            ApprovalResult = "出库成功",
                            ApprovalStage = "12",
                            CreateUser = loginUser.Name,
                            CreateUserId = loginUser.Id,
                            CreateTime = DateTime.Now,
                            QuotationId = obj.ExpressageReqs.QuotationId,
                            IntervalTime = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(selqoh.CreateTime)).TotalSeconds)
                        });
                        //if (!string.IsNullOrWhiteSpace(quotationObj.FlowInstanceId) && quotationObj.QuotationStatus != 12)
                        //{
                        //    await _flowInstanceApp.Verification(VerificationReqModle);
                        //}
                    }
                    if (isEXwarehouse == 0)
                    {
                        await UnitWork.UpdateAsync<Quotation>(q => q.Id.Equals(obj.ExpressageReqs.QuotationId), q => new Quotation { QuotationStatus = 11, UpDateTime = DateTime.Now });
                        qoh.Add(new QuotationOperationHistory
                        {
                            Id = Guid.NewGuid().ToString(),
                            Action = "出库完成",
                            ApprovalResult = "出库成功",
                            ApprovalStage = "11",
                            CreateUser = loginUser.Name,
                            CreateUserId = loginUser.Id,
                            CreateTime = DateTime.Now,
                            QuotationId = obj.ExpressageReqs.QuotationId,
                            IntervalTime = qoh.Count > 0 ? 0 : Convert.ToInt32((DateTime.Now - Convert.ToDateTime(selqoh.CreateTime)).TotalSeconds)

                        });
                        if (!string.IsNullOrWhiteSpace(quotationObj.FlowInstanceId))
                        {
                            await _flowInstanceApp.Verification(new VerificationReq
                            {
                                NodeRejectStep = "",
                                NodeRejectType = "0",
                                FlowInstanceId = quotationObj.FlowInstanceId,
                                VerificationFinally = "1",
                                VerificationOpinion = "出库成功",
                                Operator = loginUser,
                            });
                        }
                    }
                    else
                    {
                        await UnitWork.UpdateAsync<Quotation>(q => q.Id.Equals(obj.ExpressageReqs.QuotationId), q => new Quotation { QuotationStatus = 12, UpDateTime = DateTime.Now });
                    }
                    await UnitWork.BatchAddAsync<QuotationOperationHistory>(qoh.ToArray());
                    await UnitWork.SaveAsync();
                    #endregion
                    #region 返回成功值
                    var MergeMaterials = from a in QuotationMergeMaterialLists
                                         join b in LogisticsRecords on a.Id equals b.QuotationMaterialId
                                         select new { a, b };
                    result.Data = new
                    {
                        start = isEXwarehouse == 0 ? 7 : 0,
                        Expressages = Expressages.Select(e => new
                        {
                            e.ExpressagePicture,
                            e.ExpressInformation,
                            e.ExpressNumber,
                            e.Id,
                            e.Freight,
                            e.QuotationId,
                            e.Remark,
                            e.ReturnNoteId,
                            LogisticsRecords = MergeMaterials.Where(m => m.b.ExpressageId.Equals(e.Id)).Select(m => new
                            {
                                m.a.MaterialCode,
                                m.a.MaterialDescription,
                                m.a.Count,
                                m.a.Unit,
                                m.a.SentQuantity,
                                m.b.Quantity,
                                m.a.WhsCode
                            }).ToList()
                        }).ToList()
                    };
                    #endregion
                    if (obj.IsMaterialType == "4")
                    {
                        var quotation = await UnitWork.Find<Quotation>(q => q.Id == obj.Id).Include(q => q.QuotationProducts).ThenInclude(q => q.QuotationMaterials).FirstOrDefaultAsync();
                        var prodctCodes = quotation.QuotationProducts.Select(q => q.ProductCode).ToList();
                        var warrantyDates = await UnitWork.Find<SalesOrderWarrantyDate>(s => prodctCodes.Contains(s.MnfSerial)).ToListAsync();
                        foreach (var item in quotation.QuotationProducts)
                        {
                            await UnitWork.UpdateAsync<SalesOrderWarrantyDate>(s => s.MnfSerial.Equals(item.ProductCode), s => new SalesOrderWarrantyDate { WarrantyPeriod = item.WarrantyTime });
                            await UnitWork.AddAsync<SalesOrderWarrantyDateRecord>(new SalesOrderWarrantyDateRecord
                            {
                                Id = Guid.NewGuid().ToString(),
                                SalesOrderWarrantyDateId = warrantyDates.Where(w => w.MnfSerial.Equals(item.ProductCode)).FirstOrDefault()?.Id,
                                CreateTime = DateTime.Now,
                                QuotationId = quotation.Id,
                                WarrantyExpense = item.QuotationMaterials != null && item.QuotationMaterials.Count() > 0 ? item.QuotationMaterials.Sum(q => q.DiscountPrices * q.Count) : 0,
                                CreateUser = quotation.CreateUser,
                                CreateUserId = quotation.CreateUserId,
                                WarrantyPeriod = item.WarrantyTime
                            });
                        }
                        await UnitWork.SaveAsync();
                    }
                    transaction.Commit();
                    _capBus.Publish("Serve.SalesOfDelivery.Create", obj);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("添加物流失败,请重试。" + ex.Message);
                }
            }
            return result;
        }
        /// <summary>
        /// 维修费差旅费自动交货
        /// </summary>
        /// <returns></returns>
        public async Task TimeOfDelivery(int QuotationId)
        {
            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ShieldingMaterials")).Select(u => u.Name).ToListAsync();

            var quotations = await UnitWork.Find<Quotation>(q => q.Id == QuotationId && q.Status == 2).Include(q => q.QuotationMergeMaterials)
                .Where(q => (q.QuotationMergeMaterials.Where(m => !CategoryList.Contains(m.MaterialCode)).Count() <= 0 || q.IsMaterialType == 4) && q.SalesOrderId != null).FirstOrDefaultAsync();
            if (quotations != null)
            {
                var pictures = "68cc3412-492b-4f39-b7de-3ab3a957017b";
                if (quotations.IsMaterialType == 4)
                {
                    pictures = "9fda9864-6d40-46bc-a94b-3f2d45d2d3c7";
                }
                else
                {
                    if ((quotations.ServiceChargeJH > 0 || quotations.ServiceChargeSM > 0) && quotations.TravelExpense > 0)
                    {
                        pictures = "701d519b-5c0a-4369-adf4-8c0a2b7f0b16";
                    }
                    else if (quotations.TravelExpense > 0)
                    {
                        pictures = "01a62877-1961-4f0e-9f39-2dab2cb2eb4a";
                    }
                }

                AddOrUpdateQuotationReq obj = new AddOrUpdateQuotationReq();
                obj.Id = quotations.Id;
                obj.IsMaterialType = quotations.IsMaterialType.ToString();
                obj.ExpressageReqs = new ExpressageReq
                {
                    ExpressNumber = "自动出库",
                    Freight = "0",
                    QuotationId = quotations.Id,
                    ExpressagePictures = new List<string>() { pictures }
                };
                obj.QuotationMergeMaterialReqs = new List<QuotationMergeMaterialReq>();
                quotations.QuotationMergeMaterials.ForEach(q =>
                {
                    obj.QuotationMergeMaterialReqs.Add(new QuotationMergeMaterialReq
                    {
                        DiscountPrices = q.DiscountPrices,
                        MaterialDescription = q.MaterialDescription,
                        MaterialCode = q.MaterialCode,
                        WhsCode = q.WhsCode,
                        SentQuantity = q.Count,
                        QuotationId = q.QuotationId,
                        Id = q.Id,
                        MaterialType = q.MaterialType.ToString()
                    });
                });
                if (quotations.IsMaterialType != null)
                {
                    await UpdateMaterial(obj);
                }
            }
        }

        /// <summary>
        /// 审批
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Accraditation(AccraditationQuotationReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var loginUserRole = loginContext.Roles;
            if (loginUser.Account == Define.USERAPP)//兼容APP，目前App“销售员审批”环节可审批
            {
                loginUser = await GetUserId(Convert.ToInt32(req.AppId));
                loginUserRole = await GetRoleByUserId(loginUser.Id);
            }

            QuotationOperationHistory qoh = new QuotationOperationHistory();

            var obj = await UnitWork.Find<Quotation>(q => q.Id == req.Id).Include(q => q.QuotationProducts).Include(q => q.QuotationMergeMaterials).FirstOrDefaultAsync();
            if(obj.CancelRequest!=null) throw new Exception("已申请取消不可审批");
            var flowInstanceObj = await UnitWork.Find<FlowInstance>(f => f.Id.Equals(obj.FlowInstanceId)).FirstOrDefaultAsync();
            qoh.ApprovalStage = obj.QuotationStatus.ToString();

            VerificationReq VerificationReqModle = new VerificationReq
            {
                NodeRejectStep = "",
                NodeRejectType = "0",
                FlowInstanceId = obj.FlowInstanceId,
                VerificationFinally = "1",
                VerificationOpinion = req.Remark,
            };
            if (req.IsReject)
            {
                VerificationReqModle = new VerificationReq
                {
                    NodeRejectStep = "",
                    NodeRejectType = "1",
                    FlowInstanceId = obj.FlowInstanceId,
                    VerificationFinally = "3",
                    VerificationOpinion = req.Remark,
                    Operator = loginUser
                };
                if (!string.IsNullOrWhiteSpace(obj.FlowInstanceId))
                {
                    await _flowInstanceApp.Verification(VerificationReqModle);
                }
                obj.QuotationStatus = 1;
                qoh.ApprovalResult = "驳回";
                qoh.ApprovalStage = "1"; 
                if (!string.IsNullOrWhiteSpace(req.AppId.ToString()))
                    qoh.Action = loginUser.Name + "通过APP提交销售员审批";
                var delQuotationMergeMaterial = await UnitWork.Find<QuotationMergeMaterial>(q => q.QuotationId.Equals(obj.Id)).ToListAsync();
                await UnitWork.BatchDeleteAsync<QuotationMergeMaterial>(delQuotationMergeMaterial.ToArray());
            }
            else
            {
                if (flowInstanceObj != null)
                {
                    if ((loginUserRole.Any(r => r.Name.Equals("销售员")) || loginUserRole.Any(r => r.Name.Equals("总经理"))) && flowInstanceObj.ActivityName == "销售员审批")
                    {
                        qoh.Action = "销售员审批";
                        if (!string.IsNullOrWhiteSpace(req.AppId.ToString()))
                            qoh.Action = loginUser.Name+"通过APP提交销售员审批";
                        obj.QuotationStatus = 4;
                        VerificationReqModle.Operator = loginUser;
                    }
                    else if (loginUserRole.Any(r => r.Name.Equals("物料工程审批")) && flowInstanceObj.ActivityName == "工程审批")
                    {
                        qoh.Action = "工程审批";
                        obj.QuotationStatus = 5;
                    }
                    else if (loginUserRole.Any(r => r.Name.Equals("总经理")) && flowInstanceObj.ActivityName == "总经理审批")
                    {
                        qoh.Action = "总经理审批";
                        if (obj.IsMaterialType == 1 || obj.IsMaterialType == 3)
                        {
                            if (req.IsTentative == true)
                            {
                                obj.QuotationStatus = 5;
                                obj.Tentative = true;
                            }
                            else
                            {
                                obj.Tentative = false;
                                obj.QuotationStatus = 10;
                                obj.Status = 2;
                                #region 报价单同步到SAP，ERP3.0
                                _capBus.Publish("Serve.SellOrder.Create", obj.Id);
                                #endregion

                            }

                        }
                        else
                        {
                            obj.QuotationStatus = 6;
                        }

                    }
                    else if (obj.CreateUserId.Equals(loginUser.Id) && flowInstanceObj.ActivityName == "确认报价单")
                    {
                        qoh.Action = "客户确认报价单";
                        obj.QuotationStatus = 7;
                        #region 报价单同步到SAP，ERP3.0 
                        _capBus.Publish("Serve.SellOrder.Create", obj.Id);
                        #endregion
                    }
                    else if (obj.CreateUserId.Equals(loginUser.Id) && flowInstanceObj.ActivityName == "回传销售订单")
                    {
                        qoh.Action = "回传销售订单";
                        obj.QuotationStatus = 8;
                    }
                    else if (loginUserRole.Any(r => r.Name.Equals("物料财务")) && flowInstanceObj.ActivityName == "财务审批")
                    {
                        qoh.Action = "财务审批";
                        if (req.IsTentative == true)
                        {
                            obj.QuotationStatus = 8;
                            obj.Tentative = true;
                        }
                        else
                        {
                            obj.QuotationStatus = 10;
                            obj.Status = 2;
                        }

                    }
                    else if (loginUserRole.Any(r => r.Name.Equals("销售总助")) && flowInstanceObj.ActivityName == "销售总助审批")
                    {
                        qoh.Action = "销售总助审批";
                        obj.QuotationStatus = 5;
                        if (obj.WarrantyType == 1)
                        {
                            var prodctCodes = obj.QuotationProducts.Select(q => q.ProductCode).ToList();
                            var warrantyDates = await UnitWork.Find<SalesOrderWarrantyDate>(s => prodctCodes.Contains(s.MnfSerial)).ToListAsync();
                            foreach (var item in obj.QuotationProducts)
                            {
                                await UnitWork.UpdateAsync<SalesOrderWarrantyDate>(s => s.MnfSerial.Equals(item.ProductCode), s => new SalesOrderWarrantyDate { WarrantyPeriod = item.WarrantyTime });
                                await UnitWork.AddAsync<SalesOrderWarrantyDateRecord>(new SalesOrderWarrantyDateRecord
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    SalesOrderWarrantyDateId = warrantyDates.Where(w => w.MnfSerial.Equals(item.ProductCode)).FirstOrDefault()?.Id,
                                    CreateTime = DateTime.Now,
                                    QuotationId = obj.Id,
                                    WarrantyExpense = item.QuotationMaterials != null && item.QuotationMaterials.Count() > 0 ? item.QuotationMaterials.Sum(q => q.DiscountPrices * q.Count) : 0,
                                    CreateUser = obj.CreateUser,
                                    CreateUserId = obj.CreateUserId,
                                    WarrantyPeriod = item.WarrantyTime
                                });
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("暂无审批该流程权限，不可审批");
                    }
                }
                else 
                {
                    if ((loginUserRole.Any(r => r.Name.Equals("销售员")) || loginUserRole.Any(r => r.Name.Equals("总经理"))) && obj.QuotationStatus == 3.1M)
                    {
                        qoh.Action = "销售员审批";
                        obj.QuotationStatus = 4;
                    }
                    else if (loginUserRole.Any(r => r.Name.Equals("物料工程审批")) && obj.QuotationStatus == 4)
                    {
                        qoh.Action = "工程审批";
                        obj.QuotationStatus = 5;
                    }
                    else if (loginUserRole.Any(r => r.Name.Equals("总经理")) && obj.QuotationStatus == 5)
                    {
                        qoh.Action = "总经理审批";
                        if (obj.IsMaterialType == 1 || obj.IsMaterialType == 3)
                        {
                            if (req.IsTentative == true)
                            {
                                obj.QuotationStatus = 5;
                                obj.Tentative = true;
                            }
                            else
                            {
                                obj.Tentative = false;
                                obj.QuotationStatus = 10;
                                obj.Status = 2;
                                #region 报价单同步到SAP，ERP3.0
                                _capBus.Publish("Serve.SellOrder.Create", obj.Id);
                                #endregion
                            }

                        }
                        else
                        {
                            obj.QuotationStatus = 6;
                        }

                    }
                    else if (obj.CreateUserId.Equals(loginUser.Id) && obj.QuotationStatus == 6)
                    {
                        qoh.Action = "客户确认报价单";
                        obj.QuotationStatus = 7;
                        #region 报价单同步到SAP，ERP3.0 
                        _capBus.Publish("Serve.SellOrder.Create", obj.Id);
                        #endregion
                    }
                    else if (obj.CreateUserId.Equals(loginUser.Id) && obj.QuotationStatus == 7)
                    {
                        qoh.Action = "销售订单成立";
                        obj.QuotationStatus = 8;
                    }
                    else if (loginUserRole.Any(r => r.Name.Equals("物料财务")) && obj.QuotationStatus == 8)
                    {
                        qoh.Action = "财务审批";
                        if (req.IsTentative == true)
                        {
                            obj.QuotationStatus = 8;
                            obj.Tentative = true;
                        }
                        else
                        {
                            obj.QuotationStatus = 10;
                            obj.Status = 2;
                        }

                    }
                    else if (loginUserRole.Any(r => r.Name.Equals("总经理")) && obj.QuotationStatus == 9)
                    {
                        qoh.Action = "总经理审批";
                        obj.QuotationStatus = 10;
                        obj.Status = 2;
                    }
                    else
                    {
                        throw new Exception("暂无审批该流程权限，不可审批");
                    }

                }
                
                //else if (loginContext.Roles.Any(r => r.Name.Equals("总经理")) && flowInstanceObj.ActivityName == "总经理审批")
                //{
                //    qoh.Action = "总经理审批";
                //    obj.QuotationStatus = 10;
                //    obj.Status = 2;
                //}
                if (req.IsTentative == true)
                {
                    obj.QuotationStatus = decimal.Parse(qoh.ApprovalStage);
                    obj.Tentative = true;
                    qoh.ApprovalResult = "暂定";
                }
                else
                {
                    qoh.ApprovalResult = "同意";
                    
                    if (!string.IsNullOrWhiteSpace(obj.FlowInstanceId))
                    {
                        await _flowInstanceApp.Verification(VerificationReqModle);
                    }
                }

            }
            await UnitWork.UpdateAsync<Quotation>(q => q.Id == obj.Id, q => new Quotation
            {
                UpDateTime = DateTime.Now,
                Tentative = obj.Tentative,
                QuotationStatus = obj.QuotationStatus,
                Status = obj.Status,
            });
            if (req.PictureIds != null && req.PictureIds.Count > 0)
            {
                List<QuotationPicture> QuotationPictures = new List<QuotationPicture>();
                req.PictureIds.ForEach(p => QuotationPictures.Add(new QuotationPicture { Id = Guid.NewGuid().ToString(), PictureId = p, QuotationId = obj.Id }));
                await UnitWork.BatchAddAsync<QuotationPicture>(QuotationPictures.ToArray());
            }
            var selqoh = await UnitWork.Find<QuotationOperationHistory>(r => r.QuotationId.Equals(obj.Id)).OrderByDescending(r => r.CreateTime).FirstOrDefaultAsync();
            qoh.CreateUser = loginUser.Name;
            qoh.CreateUserId = loginUser.Id;
            qoh.CreateTime = DateTime.Now;
            qoh.QuotationId = obj.Id;
            qoh.Remark = req.Remark;
            qoh.IntervalTime = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(selqoh.CreateTime)).TotalSeconds);
            await UnitWork.AddAsync<QuotationOperationHistory>(qoh);
            //修改全局待处理
            await UnitWork.UpdateAsync<WorkbenchPending>(w => w.SourceNumbers == obj.Id && w.OrderType == 1, w => new WorkbenchPending
            {
                UpdateTime = obj.UpDateTime,
            });
            await UnitWork.SaveAsync();
            if (obj.Status == 2)
            {
                await TimeOfDelivery(obj.Id);
            }
        }

        /// <summary>
        /// 删除报价单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Delete(QueryQuotationListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            await UnitWork.DeleteAsync<Quotation>(q => q.Id == req.QuotationId);
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 获取用户
        /// </summary>
        /// <param name="AppId"></param>
        /// <returns></returns>
        private async Task<User> GetUserId(int AppId)
        {
            var userid = await UnitWork.Find<AppUserMap>(u => u.AppUserId.Equals(AppId)).Select(u => u.UserID).FirstOrDefaultAsync();

            return await UnitWork.Find<User>(u => u.Id.Equals(userid)).FirstOrDefaultAsync();
        }

        private async Task<List<Role>> GetRoleByUserId(string userId)
        {
            var roleId = await UnitWork.Find<Relevance>(c => c.FirstId == userId && c.Key == Define.USERROLE).Select(c => c.SecondId).ToListAsync();
            return await UnitWork.Find<Role>(c => roleId.Contains(c.Id)).ToListAsync();
        }

        /// <summary>
        /// 导入设备零件价格
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public async Task ImportMaterialPrice(ExcelHandler handler)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var MaterialPriceList = handler.GetListData<MaterialPrice>(mapper =>
            {
                var data = mapper
                .Map<MaterialPrice>(0, a => a.MaterialCode)
                .Map<MaterialPrice>(1, a => a.SettlementPrice)
                .Map<MaterialPrice>(2, a => a.SettlementPriceModel)
                .Map<MaterialPrice>(3, a => a.SalesMultiple)
                .Take<MaterialPrice>(0);
                return data.Select(d => d.Value).SkipWhile(v => v is null).ToList();
            });
            MaterialPriceList = MaterialPriceList.Where(m => !string.IsNullOrWhiteSpace(m.MaterialCode)).ToList();
            MaterialPriceList.ForEach(m =>
            {
                m.CreateUserId = loginContext.User.Id;
                m.CreateUser = loginContext.User.Name;
                m.CreateTime = DateTime.Now;
            });
            var materialCodes = MaterialPriceList.Select(m => m.MaterialCode).ToList();
            var materialPrices = await UnitWork.Find<MaterialPrice>(m => materialCodes.Contains(m.MaterialCode)).ToListAsync();
            await UnitWork.BatchDeleteAsync<MaterialPrice>(materialPrices.ToArray());
            await UnitWork.BatchAddAsync<MaterialPrice>(MaterialPriceList.ToArray());
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 通用条件
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task<string> Condition(AddOrUpdateQuotationReq obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(obj.AppId));
            }
            #region 判断技术员余额
            //var ReturnNoteList = await UnitWork.Find<ReturnNote>(r => r.CreateUserId.Equals(loginUser.Id)).Include(r => r.ReturnnoteMaterials).ToListAsync();

            //List<int> returnNoteIds = ReturnNoteList.Select(s => s.Id).Distinct().ToList();
            ////计算剩余未结清金额
            //var notClearAmountList = (await UnitWork.Find<ReturnnoteMaterial>(w => returnNoteIds.Contains((int)w.ReturnNoteId) && w.Check == 1).ToListAsync()).GroupBy(g => g.MaterialCode).Select(s => new { s.Key, GoodCount = s.Sum(s => s.GoodQty), SecondCount = s.Sum(s => s.SecondQty), DiscountPrices = s.ToList().FirstOrDefault().DiscountPrices, TotalCount = s.ToList().FirstOrDefault().TotalCount }).ToList();
            //var totalprice = notClearAmountList.Sum(s => s.DiscountPrices * (s.TotalCount - s.GoodCount - s.SecondCount));
            //if (totalprice > 4000)
            //{
            //    throw new Exception("欠款已超出额度，不可领料。");
            //}
            #endregion

            #region 判断是否存在相同物料
            List<string> MaterialCode = new List<string>();
            List<QuotationMaterialReq> QuotationMaterialReps = new List<QuotationMaterialReq>();
            List<QuotationMergeMaterial> QuotationMergeMaterials = new List<QuotationMergeMaterial>();
            StringBuilder MaterialName = new StringBuilder();

            foreach (var item in obj.QuotationProducts)
            {
                MaterialCode.AddRange(item.QuotationMaterials.Select(q => q.MaterialCode).ToList());
                QuotationMaterialReps.AddRange(item.QuotationMaterials.ToList());
            }

            QuotationMaterialReps.Where(q => q.NewMaterialCode == true).ForEach(q => { if (string.IsNullOrWhiteSpace(q.Remark)) { throw new Exception("新增物料，必须填写备注。"); } });
            var Quotations = await UnitWork.Find<Quotation>(q => q.Status > 3 && q.Status < 6).Include(q => q.QuotationMergeMaterials).Where(q => q.QuotationMergeMaterials.Any(m => MaterialCode.Contains(m.MaterialCode))).ToListAsync();

            if (Quotations != null && Quotations.Count > 0)
            {
                Quotations.ForEach(q => QuotationMergeMaterials.AddRange(q.QuotationMergeMaterials.Where(m => MaterialCode.Contains(m.MaterialCode)).ToList()));
            }
            if (QuotationMergeMaterials != null && QuotationMergeMaterials.Count > 0)
            {
                var MaterialCodeCount = QuotationMergeMaterials.GroupBy(q => q.MaterialCode).Select(q => new { q.Key, Count = q.Select(s => s.Count).Sum() });
                foreach (var item in MaterialCodeCount)
                {
                    var QuotationMaterialRepsCount = QuotationMaterialReps.Where(q => q.MaterialCode.Equals(item.Key)).Select(q => new { q.WarehouseQuantity, q.Count }).ToList();
                    if (QuotationMaterialRepsCount.Sum(q => q.Count) + item.Count > QuotationMaterialRepsCount.FirstOrDefault()?.WarehouseQuantity)
                    {
                        MaterialName.Append(QuotationMaterialRepsCount.FirstOrDefault()?.WarehouseQuantity + ",");
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(MaterialName.ToString()))
            {
                return MaterialName.ToString().Substring(0, MaterialName.ToString().Length - 2) + "已存在多笔订单且库存数量不满足，请尽快付款。";
            }
            #endregion

            #region 判断是否已经开始退料 则不允许领料
            //var isExist = (await UnitWork.Find<ReturnNote>(w => w.ServiceOrderId == obj.ServiceOrderId && w.CreateUserId == loginUser.Id).ToListAsync()).Count > 0 ? true : false;
            //if (isExist)
            //{
            //    throw new Exception("该服务单已开始退料，不可领料。");
            //}
            #endregion
            #region  判断序列号数据是否存在和序列号和物料是否匹配
            if (obj.IsMaterialType == "4")
            {
                var productCodes = obj.QuotationProducts.Where(q => q.WarrantyTime != null).Select(q => q.ProductCode).ToList();
                var warrantyDates = await UnitWork.Find<SalesOrderWarrantyDate>(s => productCodes.Contains(s.MnfSerial)).ToListAsync();
                if (warrantyDates.Count() < productCodes.Count())
                {
                    StringBuilder mnfSerials = new StringBuilder();
                    var mnfSerialList = warrantyDates.Select(w => w.MnfSerial).ToList();
                    productCodes.Where(p => !mnfSerialList.Contains(p)).ForEach(p =>
                    {
                        mnfSerials.Append(p + "、");
                    });
                    throw new Exception($"{mnfSerials.ToString().Substring(0, mnfSerials.ToString().Length - 2)}序列号暂不支持延保，请联系管理员处理");
                }
            }
            var serviceWorkOrders = await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == obj.ServiceOrderId).ToListAsync();
            obj.QuotationProducts.ForEach(q =>
            {
                if (!q.MaterialCode.Equals(serviceWorkOrders.Where(s => s.ManufacturerSerialNumber.Equals(q.ProductCode)).FirstOrDefault()?.MaterialCode))
                {
                    throw new Exception($@"{q.ProductCode}序列号与物料编码不匹配请检查后重试。");
                }
            });
            #endregion
            //判定字段是否同时存在
            if (!(!string.IsNullOrWhiteSpace(obj.TaxRate) && !string.IsNullOrWhiteSpace(obj.InvoiceCategory) && !string.IsNullOrWhiteSpace(obj.InvoiceCompany)) && !(string.IsNullOrWhiteSpace(obj.TaxRate) && string.IsNullOrWhiteSpace(obj.InvoiceCategory) && string.IsNullOrWhiteSpace(obj.InvoiceCompany)))
            {
                throw new Exception("请核对是否存在未填写字段");
            }

            //判定人员是否有销售员code
            var createUserId = loginUser.Id;
            if (!string.IsNullOrWhiteSpace(obj.CreateUserId)) 
            {
                createUserId = obj.CreateUserId;
            }
            var userId = (await UnitWork.Find<NsapUserMap>(n => n.UserID.Equals(createUserId)).FirstOrDefaultAsync())?.NsapUserId;
            var slpCode = (await UnitWork.Find<sbo_user>(s => s.user_id == userId && s.sbo_id == Define.SBO_ID).FirstOrDefaultAsync())?.sale_id;

            if (slpCode == null || slpCode == 0)
            {
                throw new Exception("暂无销售权限，请联系呼叫中心");
            }

            #region 验证客户联系人，SAP没有则新增
            if (obj.IsOutsourc == null)//因结算新增的物料单不作处理
            {
                var TerminalCustomerId = await UnitWork.Find<ServiceOrder>(c => c.Id == obj.ServiceOrderId).Select(c => c.TerminalCustomerId).FirstOrDefaultAsync();
                var contact = await UnitWork.Find<OCPR>(c => c.CardCode == TerminalCustomerId).Select(c => new { c.Name, c.Tel1 }).ToListAsync();
                if (!contact.Exists(c => c.Name == obj.NewestContacter && c.Tel1 == obj.NewestContactTel))
                {
                    //姓名+电话组合不存在的情况而名字单独存在的情况下
                    if (contact.Exists(c => c.Name == obj.NewestContacter))
                    {
                        throw new Exception("该客户已存在同名联系人。若手动修改了联系人或联系方式，请确保两个同时修改。");
                    }
                    else if (contact.Exists(c => c.Tel1 == obj.NewestContactTel))
                    {
                        throw new Exception("该客户已存在该联系方式。若手动修改了联系人或联系方式，请确保两个同时修改。");
                    }
                    else//名字和电话都不存在则新增
                    {
                        AddCoustomerContact cc = new AddCoustomerContact()
                        {
                            CardCode = TerminalCustomerId,
                            NewestContacter = obj.NewestContacter,
                            NewestContactTel = obj.NewestContactTel,
                            Address = obj.ShippingAddress
                        };
                        _capBus.Publish("Serve.OCPR.Create", cc);
                    }
                }
            }
            #endregion

            return null;
        }

        /// <summary>
        /// 计算价格
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private async Task<Quotation> CalculatePrice(AddOrUpdateQuotationReq obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (string.IsNullOrWhiteSpace(obj.WarrantyType)) obj.WarrantyType = null;
            var QuotationObj = obj.MapTo<Quotation>();
            if (QuotationObj.IsMaterialType != 4 && QuotationObj.WarrantyType == 2)
            {
                QuotationObj.QuotationProducts = QuotationObj.QuotationProducts.Where(q => q.QuotationMaterials.Count > 0).ToList();
            }
            QuotationObj.ErpOrApp = 1;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(obj.AppId));
                QuotationObj.ErpOrApp = 2;
            }
            QuotationObj.TotalMoney = 0;
            QuotationObj.TotalCommission = 0;
            QuotationObj.TotalCostPrice = 0;
            QuotationObj.Tentative = false;
            QuotationObj.PrintNo = Guid.NewGuid().ToString();
            QuotationObj.PrintTheNumber = 0;
            QuotationObj.IsProtected = QuotationObj.IsMaterialType == 2 || (QuotationObj.IsMaterialType == 4 && QuotationObj.WarrantyType == 2) ? true : false;
            QuotationObj.QuotationProducts.ForEach(q =>
            {
                q.QuotationMaterials.ForEach(m =>
                {
                    if (m.MaterialType != 4 && m.MaterialType != 3 && m.SalesPrice > 0 && Convert.ToDouble(m.DiscountPrices / m.SalesPrice) < 0.4)
                    {
                        throw new Exception($"【{q.ProductCode}】序列号下【{m.MaterialCode}】物料金额有误请重新输入");
                    }
                    m.Commission = (QuotationObj.IsMaterialType == 2 && m.MaterialType == 2) ? m.Commission : 0;
                    m.SalesPrice = m.MaterialType != 3 ? m.SalesPrice : 0;
                    m.DiscountPrices = m.MaterialType != 3 && m.MaterialType != 4 ? m.DiscountPrices : 0;
                    m.Discount = m.MaterialType != 3 && m.MaterialType != 4 ? m.SalesPrice != 0 ? (m.DiscountPrices / m.SalesPrice) * 100 : 100 : 100;
                    m.TotalPrice = m.MaterialType != 3 ? Convert.ToDecimal(Convert.ToDecimal(m.DiscountPrices * m.Count).ToString("#0.0000")) : 0;
                    QuotationObj.TotalCostPrice += m.MaterialType != 3 ? Convert.ToDecimal(Convert.ToDecimal(m.DiscountPrices * m.Count).ToString("#0.00")) : 0;
                    QuotationObj.TotalMoney += m.MaterialType != 3 ? Convert.ToDecimal(Convert.ToDecimal(m.DiscountPrices * m.Count).ToString("#0.00")) : 0;
                    //销售类型下销售物料才有佣金
                    QuotationObj.TotalCommission += (QuotationObj.IsMaterialType == 2 && m.MaterialType == 2) ? Convert.ToDecimal(Convert.ToDecimal(m.Commission).ToString("#0.00")) : 0;
                });
            });
            #region 判定通用物料
            if (QuotationObj.ServiceChargeJH != null && QuotationObj.ServiceChargeJH > 0 && QuotationObj.ServiceChargeManHourJH != null && QuotationObj.ServiceChargeManHourJH > 0 && QuotationObj.IsMaterialType == 2)
            {
                QuotationObj.TotalMoney += Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeJH * QuotationObj.ServiceChargeManHourJH).ToString("#0.00"));
                QuotationObj.TotalCommission += Convert.ToDecimal(QuotationObj.ServiceChargeJHTC.ToString("#0.00"));
                QuotationObj.ServiceChargeJHCost = 0;
            }
            else if (QuotationObj.ServiceChargeJHCost != null && QuotationObj.ServiceChargeJHCost > 0 && QuotationObj.IsMaterialType == 3)
            {
                QuotationObj.ServiceChargeJH = 0;
            }
            else
            {
                QuotationObj.ServiceChargeJH = null;
                QuotationObj.ServiceChargeJHCost = null;
                QuotationObj.ServiceChargeManHourJH = null;
            }
            if (QuotationObj.ServiceChargeSM != null && QuotationObj.ServiceChargeSM > 0 && QuotationObj.ServiceChargeManHourSM != null && QuotationObj.ServiceChargeManHourSM > 0 && QuotationObj.IsMaterialType == 2)
            {
                QuotationObj.TotalMoney += Convert.ToDecimal(Convert.ToDecimal(QuotationObj.ServiceChargeSM * QuotationObj.ServiceChargeManHourSM).ToString("#0.00"));
                QuotationObj.TotalCommission += Convert.ToDecimal(QuotationObj.ServiceChargeSMTC.ToString("#0.00"));
                QuotationObj.ServiceChargeSMCost = 0;
            }
            else if (QuotationObj.ServiceChargeSMCost != null && QuotationObj.ServiceChargeSMCost > 0 && QuotationObj.IsMaterialType == 3)
            {
                QuotationObj.ServiceChargeSM = 0;
            }
            else
            {
                QuotationObj.ServiceChargeSMCost = null;
                QuotationObj.ServiceChargeSM = null;
                QuotationObj.ServiceChargeManHourSM = null;
            }
            if (QuotationObj.TravelExpense != null && QuotationObj.TravelExpense > 0 && QuotationObj.TravelExpenseManHour != null && QuotationObj.TravelExpenseManHour > 0 && QuotationObj.IsMaterialType == 2)
            {
                QuotationObj.TotalMoney += Convert.ToDecimal(Convert.ToDecimal(QuotationObj.TravelExpense * QuotationObj.TravelExpenseManHour).ToString("#0.00"));
                QuotationObj.TotalCommission += Convert.ToDecimal(QuotationObj.TravelExpenseTC.ToString("#0.00"));
                QuotationObj.TravelExpenseCost = 0;
            }
            else if (QuotationObj.TravelExpenseCost != null && QuotationObj.TravelExpenseCost > 0 && QuotationObj.IsMaterialType == 3)
            {
                QuotationObj.TravelExpense = 0;
            }
            else
            {
                QuotationObj.TravelExpenseCost = null;
                QuotationObj.TravelExpense = null;
                QuotationObj.TravelExpenseManHour = null;
            }
            #endregion
            if (QuotationObj.IsMaterialType == 4)
            {
                QuotationObj.QuotationProducts = QuotationObj.QuotationProducts.Where(q => q.WarrantyTime != null).ToList();
            }
            return QuotationObj;
        }

        /// <summary>
        /// 获取合并后数据
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetMergeMaterial(QueryQuotationListReq req)
        {
            var result = new TableData();
            var QuotationMergeMaterials = await UnitWork.Find<QuotationMergeMaterial>(q => q.QuotationId.Equals(req.QuotationId))
                .WhereIf(!string.IsNullOrWhiteSpace(req.MaterialCode), q => req.MaterialCode.Contains(q.MaterialCode)).ToListAsync();
            //result.Count = await QuotationMergeMaterials.CountAsync();
            var MaterialsList = QuotationMergeMaterials.Select(q => q.MaterialCode).ToList();
            var WhsCode = QuotationMergeMaterials.Select(q => q.WhsCode).Distinct().ToList();
            var ItemCodes = await UnitWork.Find<OITW>(o => MaterialsList.Contains(o.ItemCode) && WhsCode.Contains(o.WhsCode)).Select(o => new { o.ItemCode, o.WhsCode, o.OnHand }).ToListAsync();
            result.Data = QuotationMergeMaterials.Select(q => new
            {
                WhsCode = q.WhsCode,
                WarehouseQuantity = ItemCodes.Where(i => i.ItemCode.Equals(q.MaterialCode) && i.WhsCode.Equals(q.WhsCode)).FirstOrDefault()?.OnHand,
                q.MaterialCode,
                q.MaterialDescription,
                q.MaterialType,
                q.QuotationId,
                q.SentQuantity,
                q.Count,
                q.Unit,
                q.Id
            }).OrderBy(q => q.MaterialCode).ToList();
            return result;
        }

        public async Task<string> PrintSalesOrder(int saleOrderId)
        {
            var quaotion = await UnitWork.Find<Quotation>(c => c.SalesOrderId == saleOrderId).FirstOrDefaultAsync();
            var category = await UnitWork.Find<Category>(c => c.TypeId == "SYS_InvoiceCompany" && c.DtValue == quaotion.InvoiceCompany).FirstOrDefaultAsync();
            var contractFile = "";
            if (category != null)
            {
                HttpHelper httpHelper = new HttpHelper(_appConfiguration.Value.ERP3Url);
                var resultApi = httpHelper.Get<Dictionary<string, string>>(new Dictionary<string, string> { { "DocEntry", saleOrderId.ToString() }, { "Indicator", category.DtCode } }, "/spv/exportsaleorder.ashx");
                if (resultApi["msg"] == "success")
                {
                    contractFile = resultApi["url"].Replace("192.168.0.208", "218.17.149.195").ToString();
                    return contractFile;
                }
                else
                {
                    return contractFile;
                }
            }
            return contractFile;
        }

        /// <summary>
        /// 打印销售订单
        /// </summary>
        /// <param name="QuotationId"></param>
        /// <returns></returns>
        public async Task<TableData> PrintSalesOrder(string SaleOrderId)
        {
            TableData result = new TableData();
            var quotation = await UnitWork.Find<Quotation>(c => c.SalesOrderId == int.Parse(SaleOrderId)).FirstOrDefaultAsync();
            if (quotation==null)
            {
                throw new Exception("领料单不存在");
            }
            var category = await UnitWork.Find<Category>(c => c.TypeId == "SYS_InvoiceCompany" && c.DtValue == quotation.InvoiceCompany).FirstOrDefaultAsync();
            if (category==null)
            {
                throw new Exception("开票单位不存在");
            }
            HttpHelper httpHelper = new HttpHelper(_appConfiguration.Value.ERP3Url);
            var resultApi = httpHelper.Get<Dictionary<string, string>>(new Dictionary<string, string> { { "DocEntry", quotation.SalesOrderId.ToString() }, { "Indicator", category.DtCode }, { "DataType", "SaleOrder" } }, "/spv/exportsaleorder.ashx");
            if (resultApi["msg"] == "success")
            {
                var url = resultApi["url"].Replace("192.168.0.208", "218.17.149.195");
                result.Data = url;
            }
            else
            {
                result.Code = 500;
                result.Message = resultApi["msg"];
            }
            #region MyRegion
            //var quotationId = int.Parse(QuotationId);
            //var model = await UnitWork.Find<Quotation>(q => q.Id.Equals(quotationId) && q.QuotationStatus < 10).Include(q => q.QuotationMergeMaterials).Include(q => q.QuotationOperationHistorys).FirstOrDefaultAsync();
            //if (model != null || model == null)
            //{
            //    throw new Exception("暂未开放销售订单打印，请前往3.0打印。");
            //    //throw new Exception("已出库，不可打印。");
            //}
            //var serverOrder = await UnitWork.Find<ServiceOrder>(q => q.Id.Equals(model.ServiceOrderId)).FirstOrDefaultAsync();
            //var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_AcquisitionWay") || u.TypeId.Equals("SYS_DeliveryMethod")).Select(u => new { u.Name, u.TypeId, u.DtValue, u.Description }).ToListAsync();
            //var createTime = Convert.ToDateTime(model.QuotationOperationHistorys.Where(q => q.ApprovalStage == "6.0").FirstOrDefault()?.CreateTime).ToString("yyyy.MM.dd");
            //var url = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "SalesOrderHeader.html");
            //var text = System.IO.File.ReadAllText(url);
            //text = text.Replace("@Model.SalesOrderId", model.SalesOrderId.ToString());
            //text = text.Replace("@Model.CreateTime", createTime);
            //text = text.Replace("@Model.SalesUser", model?.CreateUser.ToString());
            //text = text.Replace("@Model.QRcode", QRCoderHelper.CreateQRCodeToBase64(model.SalesOrderId.ToString()));
            //text = text.Replace("@Model.CustomerId", serverOrder?.TerminalCustomerId.ToString());
            //text = text.Replace("@Model.CollectionAddress", model?.CollectionAddress.ToString());
            //text = text.Replace("@Model.ShippingAddress", model?.ShippingAddress.ToString());
            //text = text.Replace("@Model.CustomerName", serverOrder?.TerminalCustomer.ToString());
            //text = text.Replace("@Model.NewestContacter", serverOrder?.NewestContacter.ToString());
            //text = text.Replace("@Model.NewestContactTel", serverOrder?.NewestContactTel.ToString());
            //text = text.Replace("@Model.DeliveryMethod", CategoryList.Where(c => c.DtValue.Equals(model?.DeliveryMethod) && c.TypeId.Equals("SYS_DeliveryMethod")).FirstOrDefault()?.Name);
            //text = text.Replace("@Model.AcquisitionWay", CategoryList.Where(c => c.DtValue.Equals(model?.AcquisitionWay) && c.TypeId.Equals("SYS_AcquisitionWay")).FirstOrDefault()?.Name);
            //text = text.Replace("@Model.DeliveryDate", Convert.ToDateTime(model?.DeliveryDate).ToString("yyyy.MM.dd"));
            //text = text.Replace("@Model.AcceptancePeriod", Convert.ToDateTime(model?.DeliveryDate).AddDays(model.AcceptancePeriod == null ? 0 : (double)model.AcceptancePeriod).ToString("yyyy.MM.dd"));
            //text = text.Replace("@Model.Remark", model?.Remark);
            //string InvoiceCompany = "", Location = "", website = "", seal = "", width = "", height = "";

            //if (Convert.ToInt32(model.InvoiceCompany) == 1)
            //{
            //    InvoiceCompany = "深圳市新威尔电子有限公司 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; 交通银行股份有限公司&nbsp;&nbsp;深圳梅林支行 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;443066388018001726113";
            //    Location = "深圳市福田区梅林街道梅都社区中康路 128 号卓越梅林中心广场(北区)3 号楼 1206 电话：0755-83108866 免费服务专线：800-830-8866";
            //    website = "www.neware.com.cn &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
            //    seal = "新威尔";
            //    width = "350px";
            //    height = "350px";
            //}
            //else if (Convert.ToInt32(model.InvoiceCompany) == 2)
            //{
            //    InvoiceCompany = "东莞新威检测技术有限公司 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; 交通银行股份有限公司 &nbsp;&nbsp; 东莞塘厦支行 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;483007618018810043352";
            //    Location = "广东省东莞市塘厦镇龙安路5号5栋101室";
            //    seal = "东莞新威";
            //    width = "182px";
            //    height = "193px";
            //    text = text.Replace("@Model.logo", "hidden='hidden'");
            //}
            //var tempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"SalesOrderHeader{model.Id}.html");
            //System.IO.File.WriteAllText(tempUrl, text, Encoding.Unicode);
            //var footUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "SalesOrderFooter.html");
            //var foottext = System.IO.File.ReadAllText(footUrl);
            //foottext = foottext.Replace("@Model.Corporate", InvoiceCompany);
            //foottext = foottext.Replace("@Model.PrintNo", model.PrintNo);
            //foottext = foottext.Replace("@Model.Location", Location);
            //foottext = foottext.Replace("@Model.Website", website);
            //foottext = foottext.Replace("@Model.PrintTheNumber", (model.PrintTheNumber + 1).ToString());
            //foottext = foottext.Replace("@Model.seal", seal);
            //foottext = foottext.Replace("@Model.width", width);
            //foottext = foottext.Replace("@Model.height", height);
            //var foottempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"SalesOrderFooter{model.Id}.html");
            //System.IO.File.WriteAllText(foottempUrl, foottext, Encoding.Unicode);
            //var materials = model.QuotationMergeMaterials.Select(q => new PrintSalesOrderResp
            //{
            //    MaterialCode = q.MaterialCode,
            //    MaterialDescription = q.MaterialDescription,
            //    Count = q.Count.ToString(),
            //    Unit = q.Unit,
            //    SalesPrice = q.MaterialType == 1 ? 0.00M : (decimal)q.DiscountPrices,
            //    TotalPrice = q.MaterialType == 1 ? 0.00M : (decimal)q.TotalPrice
            //}).OrderBy(m => m.MaterialCode).ToList();
            //var datas = await ExportAllHandler.Exporterpdf(materials, "PrintSalesOrder.cshtml", pdf =>
            //{
            //    pdf.IsWriteHtml = true;
            //    pdf.PaperKind = PaperKind.A4;
            //    pdf.Orientation = Orientation.Portrait;
            //    pdf.IsEnablePagesCount = true;
            //    pdf.HeaderSettings = new HeaderSettings() { HtmUrl = tempUrl };
            //    pdf.FooterSettings = new FooterSettings() { HtmUrl = foottempUrl };
            //});
            //System.IO.File.Delete(tempUrl);
            //System.IO.File.Delete(foottempUrl);
            //await UnitWork.UpdateAsync<Quotation>(q => q.Id.Equals(quotationId), q => new Quotation { PrintTheNumber = q.PrintTheNumber + 1 });
            //await UnitWork.SaveAsync();
            #endregion
            return result;
        }

        /// <summary>
        /// 打印报价单
        /// </summary>
        /// <param name="QuotationId"></param>
        /// <returns></returns>
        public async Task<byte[]> PrintQuotation(string QuotationId)
        {
            var quotationId = int.Parse(QuotationId);
            var model = await UnitWork.Find<Quotation>(q => q.Id.Equals(quotationId)).Include(q => q.QuotationMergeMaterials).Include(q => q.QuotationOperationHistorys).FirstOrDefaultAsync();
            var serverOrder = await UnitWork.Find<ServiceOrder>(q => q.Id.Equals(model.ServiceOrderId)).FirstOrDefaultAsync();
            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_AcquisitionWay") || u.TypeId.Equals("SYS_DeliveryMethod")).Select(u => new { u.Name, u.TypeId, u.DtValue, u.Description }).ToListAsync();
            //动态生成临时页头
            var url = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Quotationheader.html");
            var text = System.IO.File.ReadAllText(url);
            text = text.Replace("@Model.QuotationId", model.Id.ToString());
            text = text.Replace("@Model.CreateTime", model.CreateTime.ToString("yyyy.MM.dd hh:mm"));
            text = text.Replace("@Model.SalesUser", model?.CreateUser.ToString());
            text = text.Replace("@Model.QRcode", QRCoderHelper.CreateQRCodeToBase64(model.Id.ToString()));
            text = text.Replace("@Model.CustomerId", serverOrder?.TerminalCustomerId.ToString());
            text = text.Replace("@Model.CollectionAddress", model?.CollectionAddress.ToString());
            text = text.Replace("@Model.ShippingAddress", model?.ShippingAddress.ToString());
            text = text.Replace("@Model.CustomerName", serverOrder?.TerminalCustomer.ToString());
            text = text.Replace("@Model.NewestContacter", serverOrder?.NewestContacter.ToString());
            text = text.Replace("@Model.NewestContactTel", serverOrder?.NewestContactTel.ToString());
            text = text.Replace("@Model.DeliveryMethod", CategoryList.Where(c => c.DtValue.Equals(model?.DeliveryMethod) && c.TypeId.Equals("SYS_DeliveryMethod")).FirstOrDefault()?.Name);
            text = text.Replace("@Model.AcquisitionWay", CategoryList.Where(c => c.DtValue.Equals(model?.AcquisitionWay) && c.TypeId.Equals("SYS_AcquisitionWay")).FirstOrDefault()?.Name);
            text = text.Replace("@Model.DeliveryDate", Convert.ToDateTime(model?.DeliveryDate).ToString("yyyy.MM.dd"));
            text = text.Replace("@Model.AcceptancePeriod", Convert.ToDateTime(model?.DeliveryDate).AddDays(model.AcceptancePeriod == null ? 0 : (double)model.AcceptancePeriod).ToString("yyyy.MM.dd"));
            text = text.Replace("@Model.Remark", model?.Remark);
            var tempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"Quotationheader{model.Id}.html");
            System.IO.File.WriteAllText(tempUrl, text);
            //动态生成临时页脚
            var footerUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Quotationfooter.html");
            var footerText = System.IO.File.ReadAllText(footerUrl);
            var invoiceCompany = await UnitWork.Find<Category>(c => c.TypeId == "SYS_InvoiceCompany" && c.DtValue == model.InvoiceCompany).FirstOrDefaultAsync();
            footerText = footerText.Replace("@Model.Company", invoiceCompany?.Name);
            footerText = footerText.Replace("@Model.Address", invoiceCompany?.Description);
            var tempFooterUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"Quotationfooter{model.Id}.html");
            System.IO.File.WriteAllText(tempFooterUrl, footerText);
            //上门维修费、寄回维修费、差旅费数量显示为1,单价等于总价
            var specialMaterials = new string[] { "S111-SERVICE-GSF-SM", "S111-SERVICE-GSF-JH", "S111-SERVICE-CLF" }; 
            var materials = model.QuotationMergeMaterials.Select(q => new PrintSalesOrderResp
            {
                MaterialCode = q.MaterialCode,
                MaterialDescription = q.MaterialDescription,
                Count = specialMaterials.Contains(q.MaterialCode) ? "1" : q.Count.ToString(),
                Unit = q.Unit,
                SalesPrice = specialMaterials.Contains(q.MaterialCode) ? (decimal)q.TotalPrice : (q.MaterialType == 1 ? 0.00M : (decimal)q.DiscountPrices),
                TotalPrice = q.MaterialType == 1 ? 0.00M : (decimal)q.TotalPrice
            }).OrderBy(q => q.MaterialCode).ToList();
            //打印
            var datas = await ExportAllHandler.Exporterpdf(materials, "PrintQuotation.cshtml", pdf =>
            {
                pdf.IsWriteHtml = true;
                pdf.PaperKind = PaperKind.A4;
                pdf.Orientation = Orientation.Portrait;
                pdf.HeaderSettings = new HeaderSettings() { HtmUrl = tempUrl };
                pdf.FooterSettings = new FooterSettings() { HtmUrl = tempFooterUrl };
            });
            //删除临时文件
            System.IO.File.Delete(tempUrl);
            System.IO.File.Delete(tempFooterUrl);

            return datas;
        }

        /// <summary>
        /// 打印交货单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task PrintStockRequisition(List<QuotationMergeMaterialReq> req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var model = await UnitWork.Find<Quotation>(q => q.Id == req.FirstOrDefault().QuotationId && q.QuotationStatus >= 10).Include(q => q.QuotationOperationHistorys).FirstOrDefaultAsync();
            var QuotationMergeMaterial = new List<QuotationMergeMaterial>();
            if (loginContext.Roles.Any(r => r.Name.Equals("仓库")) && req.Count > 0)
            {
                var ids = req.Select(m => m.Id).ToList();
                QuotationMergeMaterial = await UnitWork.Find<QuotationMergeMaterial>(q => ids.Contains(q.Id)).ToListAsync();
                QuotationMergeMaterial.ForEach(q =>
                {
                    q.Count = req.Where(m => m.Id == q.Id).FirstOrDefault().SentQuantity;
                });
            }
            else
            {
                QuotationMergeMaterial = await UnitWork.Find<QuotationMergeMaterial>(q => q.QuotationId == req.FirstOrDefault().QuotationId).ToListAsync();
            }
            if (model != null)
            {
                var serverOrder = await UnitWork.Find<ServiceOrder>(q => q.Id.Equals(model.ServiceOrderId)).FirstOrDefaultAsync();
                //var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_AcquisitionWay") || u.TypeId.Equals("SYS_DeliveryMethod")).Select(u => new { u.Name, u.TypeId, u.DtValue, u.Description }).ToListAsync();
                var SecondId = (await UnitWork.Find<Relevance>(r => r.FirstId.Equals(model.CreateUserId) && r.Key.Equals(Define.USERORG)).FirstOrDefaultAsync()).SecondId;
                var orgName = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(o => o.Id.Equals(SecondId)).Select(o => o.Name).FirstOrDefaultAsync();

                //var createTime = Convert.ToDateTime(model.QuotationOperationHistorys.Where(q => q.ApprovalStage.Equals("4")).FirstOrDefault()?.CreateTime).ToString("yyyy.MM.dd");
                var url = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "StockRequisitionHeader.html");
                var text = System.IO.File.ReadAllText(url);
                text = text.Replace("@Model.PickingList", model.Id.ToString());
                text = text.Replace("@Model.CreateTime", DateTime.Now.ToString("yyyy.MM.dd"));
                text = text.Replace("@Model.QRcode", QRCoderHelper.CreateQRCodeToBase64(model.Id.ToString()));
                text = text.Replace("@Model.OrgName", orgName);
                text = text.Replace("@Model.NewestContacter", model.NewestContacter);
                text = text.Replace("@Model.NewestContactTel", model.NewestContactTel);
                text = text.Replace("@Model.ShippingAddress", model.ShippingAddress + model.ShippingDA);
                text = text.Replace("@Model.CreateUser", model.CreateUser);
                var tempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"StockRequisitionHeader{model.Id}.html");
                System.IO.File.WriteAllText(tempUrl, text, Encoding.Unicode);
                var footUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "StockRequisitionFooter.html");
                var foottext = System.IO.File.ReadAllText(footUrl);
                foottext = foottext.Replace("@Model.User", loginContext.User.Name);
                var foottempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"StockRequisitionFooter{model.Id}.html");
                System.IO.File.WriteAllText(foottempUrl, foottext, Encoding.Unicode);
                var materialList = QuotationMergeMaterial.Select(m => m.MaterialCode).ToList();
                var locationList = await UnitWork.Query<v_storeitemstock>(@$"select ItemCode,layer_no,unit_no,shelf_nm from v_storeitemstock").Where(v => materialList.Contains(v.ItemCode)).Select(v => new v_storeitemstock { ItemCode = v.ItemCode, layer_no = v.shelf_nm + "-" + v.layer_no + "-" + v.unit_no }).ToListAsync();

                var materials = QuotationMergeMaterial.Select(q => new PrintSalesOrderResp
                {
                    MaterialCode = q.MaterialCode,
                    MaterialDescription = q.MaterialDescription,
                    Count = q.Count.ToString(),
                    Unit = q.Unit,
                    ServiceOrderSapId = model.ServiceOrderSapId.ToString(),
                    SalesOrder = model.SalesOrderId.ToString(),
                    WhsCode = q.WhsCode,
                    Location = locationList.Where(l => l.ItemCode.Equals(q.MaterialCode)).FirstOrDefault()?.layer_no
                }).OrderBy(q => q.MaterialCode).ToList();

                var datas = await ExportAllHandler.Exporterpdf(materials, "StockRequisitionList.cshtml", pdf =>
                {
                    pdf.IsWriteHtml = true;
                    pdf.PaperKind = PaperKind.A5;
                    pdf.IsEnablePagesCount = true;
                    pdf.HeaderSettings = new HeaderSettings() { HtmUrl = tempUrl };
                    pdf.FooterSettings = new FooterSettings() { HtmUrl = foottempUrl, Right = "[page]/[toPage]" };
                });
                System.IO.File.Delete(tempUrl);
                System.IO.File.Delete(foottempUrl);
                await RedisHelper.AppendAsync(req.FirstOrDefault().QuotationId.ToString(), datas);
            }
            else
            {
                throw new Exception("暂无此领料单，请核对后重试。");
            }
        }

        /// <summary>
        /// 打印交货单
        /// </summary>
        /// <param name="QuotationId"></param>
        /// <returns></returns>
        public async Task<byte[]> PrintStockRequisition(string QuotationId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var b = await RedisHelper.GetAsync<byte[]>(QuotationId);
            await RedisHelper.DelAsync(QuotationId);
            var IsPrintWarehouse = (await UnitWork.Find<Quotation>(q => q.Id == int.Parse(QuotationId)).FirstOrDefaultAsync())?.PrintWarehouse;
            if (IsPrintWarehouse == null || IsPrintWarehouse != 3)
            {
                await UnitWork.UpdateAsync<Quotation>(q => q.Id == int.Parse(QuotationId), q => new Quotation { PrintWarehouse = 3 });
            }
            await UnitWork.AddAsync<QuotationOperationHistory>(new QuotationOperationHistory
            {
                Action = "仓库打印",
                ApprovalStage = "-1",
                CreateTime = DateTime.Now,
                CreateUser = loginContext.User.Name,
                CreateUserId = loginContext.User.Id,
                QuotationId = int.Parse(QuotationId)
            });
            await UnitWork.SaveAsync();
            return b;
        }

        /// <summary>
        /// 打印装箱清单
        /// </summary>
        /// <param name="QuotationId"></param>
        /// <param name="IsTrue"></param>
        /// <returns></returns>
        public async Task<byte[]> PrintPickingList(string QuotationId, bool? IsTrue)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            List<LogisticsRecord> logisticsRecords = new List<LogisticsRecord>();
            string Action = "技术员打印";
            if (!(bool)IsTrue)
            {
                Action = "仓库打印";
                var expressageList = await UnitWork.Find<Expressage>(e => e.Id.Equals(QuotationId)).Include(e => e.LogisticsRecords).FirstOrDefaultAsync();
                QuotationId = expressageList.QuotationId.ToString();
                logisticsRecords = expressageList.LogisticsRecords.ToList();
            }
            var quotationId = int.Parse(QuotationId);
            var model = await UnitWork.Find<Quotation>(q => q.Id.Equals(quotationId)).Include(q => q.QuotationMergeMaterials).FirstOrDefaultAsync();
            var serverOrder = await UnitWork.Find<ServiceOrder>(q => q.Id.Equals(model.ServiceOrderId)).FirstOrDefaultAsync();
            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_AcquisitionWay") || u.TypeId.Equals("SYS_DeliveryMethod")).Select(u => new { u.Name, u.TypeId, u.DtValue, u.Description }).ToListAsync();

            var url = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "PickingListHeader.html");
            var text = System.IO.File.ReadAllText(url);
            text = text.Replace("@Model.QuotationId", model.Id.ToString());
            text = text.Replace("@Model.SalesOrderId", model.SalesOrderId.ToString());
            text = text.Replace("@Model.CreateTime", DateTime.Now.ToString("yyyy.MM.dd HH:mm"));//model.CreateTime.ToString("yyyy.MM.dd hh:mm")
            text = text.Replace("@Model.SalesUser", model?.CreateUser.ToString());
            text = text.Replace("@Model.QRcode", QRCoderHelper.CreateQRCodeToBase64(model.Id.ToString()));
            text = text.Replace("@Model.CustomerId", serverOrder?.TerminalCustomerId.ToString());
            text = text.Replace("@Model.CollectionAddress", model?.CollectionAddress.ToString());
            text = text.Replace("@Model.ShippingAddress", model?.ShippingAddress.ToString());
            text = text.Replace("@Model.CustomerName", serverOrder?.TerminalCustomer.ToString());
            text = text.Replace("@Model.NewestContacter", serverOrder?.NewestContacter.ToString());
            text = text.Replace("@Model.NewestContactTel", serverOrder?.NewestContactTel.ToString());
            text = text.Replace("@Model.DeliveryMethod", CategoryList.Where(c => c.DtValue.Equals(model?.DeliveryMethod) && c.TypeId.Equals("SYS_DeliveryMethod")).FirstOrDefault()?.Name);
            text = text.Replace("@Model.AcquisitionWay", CategoryList.Where(c => c.DtValue.Equals(model?.AcquisitionWay) && c.TypeId.Equals("SYS_AcquisitionWay")).FirstOrDefault()?.Name);
            text = text.Replace("@Model.DeliveryDate", Convert.ToDateTime(model?.DeliveryDate).ToString("yyyy.MM.dd"));
            text = text.Replace("@Model.AcceptancePeriod", Convert.ToDateTime(model?.DeliveryDate).AddDays(model.AcceptancePeriod == null ? 0 : (double)model.AcceptancePeriod).ToString("yyyy.MM.dd"));
            text = text.Replace("@Model.Remark", model?.Remark);
            var tempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"PickingListHeader{model.Id}.html");
            System.IO.File.WriteAllText(tempUrl, text, Encoding.Unicode);
            var footerUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"PickingListFooter.html");
            text = System.IO.File.ReadAllText(footerUrl);
            text = text.Replace("@Model.UserName", loginContext.User.Name);
            footerUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"PickingListFooter{model.Id}.html");
            System.IO.File.WriteAllText(footerUrl, text, Encoding.Unicode);
            if (logisticsRecords.Count > 0)
            {
                var ids = logisticsRecords.Select(l => l.QuotationMaterialId).ToList();
                model.QuotationMergeMaterials = model.QuotationMergeMaterials.Where(q => ids.Contains(q.Id)).Select(q => new QuotationMergeMaterial
                {
                    MaterialCode = q.MaterialCode,
                    MaterialDescription = q.MaterialDescription,
                    Count = logisticsRecords.Where(l => l.QuotationMaterialId.Equals(q.Id)).FirstOrDefault()?.Quantity,
                    Unit = q.Unit,
                    WhsCode = q.WhsCode
                }).ToList();
            }
            var materials = model.QuotationMergeMaterials.Select(q => new PrintSalesOrderResp
            {
                MaterialCode = q.MaterialCode,
                MaterialDescription = q.MaterialDescription,
                Count = q.Count.ToString(),
                Unit = q.Unit,
                WhsCode = q.WhsCode
            }).OrderBy(q => q.MaterialCode).ToList();
            var datas = await ExportAllHandler.Exporterpdf(materials, "PrintPickingList.cshtml", pdf =>
            {
                pdf.IsWriteHtml = true;
                pdf.PaperKind = PaperKind.A4;
                pdf.Orientation = Orientation.Portrait;
                pdf.HeaderSettings = new HeaderSettings() { HtmUrl = tempUrl };
                pdf.FooterSettings = new FooterSettings() { HtmUrl = footerUrl };
            });
            System.IO.File.Delete(tempUrl);
            System.IO.File.Delete(footerUrl);
            var IsPrintWarehouse = (await UnitWork.Find<Quotation>(q => q.Id == int.Parse(QuotationId)).FirstOrDefaultAsync())?.PrintWarehouse;
            if (IsPrintWarehouse == null || IsPrintWarehouse != 3)
            {
                await UnitWork.UpdateAsync<Quotation>(q => q.Id == int.Parse(QuotationId), q => new Quotation { PrintWarehouse = 2 });
            }
            await UnitWork.AddAsync<QuotationOperationHistory>(new QuotationOperationHistory
            {
                Action = Action,
                ApprovalStage = "-1",
                CreateTime = DateTime.Now,
                CreateUser = loginContext.User.Name,
                CreateUserId = loginContext.User.Id,
                QuotationId = int.Parse(QuotationId)
            });
            await UnitWork.SaveAsync();
            return datas;
        }

        /// <summary>
        /// 同步销售订单
        /// </summary>
        /// <param name="QuotationId"></param>
        /// <returns></returns>
        public async Task SyncSalesOrder(string QuotationId)
        {
            _capBus.Publish("Serve.SellOrder.ERPCreate", int.Parse(QuotationId));
        }
        public async Task SyncSalesOrderToSap(string QuotationId)
        {
            _capBus.Publish("Serve.SellOrder.Create", int.Parse(QuotationId));
        }


        /// <summary>
        /// 同步销售交货
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task SyncSalesOfDelivery(QueryQuotationListReq request)
        {
            //_capBus.Publish("Serve.SalesOfDelivery.ERPCreate", int.Parse(request.SalesOfDeliveryId));

            var docNum = int.Parse(request.SalesOfDeliveryId);
            string Message = "";
            try
            {
                var ODLNmodel = await UnitWork.Find<ODLN>(o => o.DocEntry == docNum).ToListAsync();
                var DLN1model = await UnitWork.Find<DLN1>(o => o.DocEntry == docNum).ToListAsync();
                foreach (var item in ODLNmodel)
                {
                    if (item.PartSupply == "true") { item.PartSupply = "Y"; } else { item.PartSupply = "N"; }

                    if (item.DocCur == "") { item.DocCur = "RMB"; }

                    //if (dtRowODLN.Rows[0][3].ToString() == "") { model.DocRate = 1; }

                    if (item.DocTotal == null) { item.DocTotal = 0; }

                    if (item.OwnerCode == null) { item.OwnerCode = -1; }

                    if (item.SlpCode == null) { item.SlpCode = -1; }
                }


                List<sale_odln> sale_Odln = ODLNmodel.MapToList<sale_odln>();
                List<sale_dln1> sale_Dln1 = DLN1model.MapToList<sale_dln1>();
                sale_Odln.ForEach(s => s.sbo_id = Define.SBO_ID);
                sale_Dln1.ForEach(s => s.sbo_id = Define.SBO_ID);
                await UnitWork.BatchAddAsync<sale_odln, int>(sale_Odln.ToArray());
                await UnitWork.BatchAddAsync<sale_dln1, int>(sale_Dln1.ToArray());

                List<string> itemcodes = sale_Dln1.Select(s => s.ItemCode).ToList();
                List<string> WhsCodes = sale_Dln1.Select(s => s.WhsCode).ToList();
                var oitwList = await UnitWork.Find<OITW>(o => itemcodes.Contains(o.ItemCode) && WhsCodes.Contains(o.WhsCode)).Select(o => new { o.ItemCode, o.IsCommited, o.OnHand, o.OnOrder }).ToListAsync();
                foreach (var item in oitwList)
                {
                    var WhsCode = sale_Dln1.Where(q => q.ItemCode.Equals(item.ItemCode)).FirstOrDefault().WhsCode;
                    await UnitWork.UpdateAsync<store_oitw>(o => o.sbo_id == Define.SBO_ID && o.ItemCode == item.ItemCode && o.WhsCode == WhsCode, o => new store_oitw
                    {
                        OnHand = item.OnHand,
                        IsCommited = item.IsCommited,
                        OnOrder = item.OnOrder
                    });
                }
                var oitmList = await UnitWork.Find<OITM>(o => itemcodes.Contains(o.ItemCode)).Select(o => new { o.ItemCode, o.IsCommited, o.OnHand, o.OnOrder, o.LastPurCur, o.LastPurPrc, o.LastPurDat, o.UpdateDate }).ToListAsync();
                foreach (var item in oitmList)
                {
                    await UnitWork.UpdateAsync<store_oitm>(o => o.sbo_id == Define.SBO_ID && o.ItemCode == item.ItemCode, o => new store_oitm
                    {
                        OnHand = item.OnHand,
                        IsCommited = item.IsCommited,
                        OnOrder = item.OnOrder,
                        LastPurDat = item.LastPurDat,
                        LastPurPrc = item.LastPurPrc,
                        LastPurCur = item.LastPurCur,
                        UpdateDate = item.UpdateDate
                    });
                }
                await UnitWork.SaveAsync();
            }
            catch (Exception e)
            {
                //Message = $"添加销售交货:{docNum}到erp3.0时异常！错误信息：" + e.Message;
            }
            if (Message != "")
            {
                throw new Exception(Message.ToString());
            }
        }

        /// <summary>
        /// 清空交货记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task EmptyDeliveryRecord(QueryQuotationListReq request)
        {
            var expressages = await UnitWork.Find<Expressage>(e => e.QuotationId == request.QuotationId).Include(e => e.ExpressagePicture).Include(e => e.LogisticsRecords).ToListAsync();
            var picture = new List<ExpressagePicture>();
            expressages.ForEach(e => picture.AddRange(e.ExpressagePicture));
            var logisticsRecords = new List<LogisticsRecord>();
            expressages.ForEach(e => logisticsRecords.AddRange(e.LogisticsRecords));
            await UnitWork.BatchDeleteAsync<ExpressagePicture>(picture.ToArray());
            await UnitWork.BatchDeleteAsync<LogisticsRecord>(logisticsRecords.ToArray());
            await UnitWork.BatchDeleteAsync<Expressage>(expressages.ToArray());
            await UnitWork.UpdateAsync<Quotation>(q => q.Id == request.QuotationId, q => new Quotation { QuotationStatus = 10 });
            await UnitWork.UpdateAsync<QuotationMergeMaterial>(q => q.QuotationId == request.QuotationId, q => new QuotationMergeMaterial { SentQuantity = 0 });
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 取消销售订单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task CancellationSalesOrder(QueryQuotationListReq request)
        {
            if (request.IsReject != null && (bool)request.IsReject)
            {
                await UnitWork.UpdateAsync<Quotation>(q => q.Id == request.QuotationId,q=>new Quotation { CancelRequest=null});
                await UnitWork.SaveAsync();
            }
            else 
            {
                _capBus.Publish("Serve.SellOrder.Cancel", request.QuotationId);
                var quotationObj = await UnitWork.Find<Quotation>(q => q.Id == request.QuotationId).FirstOrDefaultAsync();
                if (!string.IsNullOrWhiteSpace(quotationObj.FlowInstanceId))
                {
                    await _flowInstanceApp.ReCall(new RecallFlowInstanceReq { FlowInstanceId = quotationObj.FlowInstanceId });
                }
            }
            
        }

        /// <summary>
        /// 同步销售订单
        /// </summary>
        /// <returns></returns>
        public async Task SyncSalesOrderStatus()
        {
            var salesOrderIds = await UnitWork.Find<Quotation>(q => !string.IsNullOrWhiteSpace(q.SalesOrderId.ToString()) && q.QuotationStatus != -1M && q.QuotationStatus!=11).Select(q => q.SalesOrderId).ToListAsync();
            var oRDRS = await UnitWork.Find<ORDR>(o => salesOrderIds.Contains(o.DocEntry) && (o.DocStatus == "C" || o.CANCELED == "Y")).Select(o => new { o.DocEntry, o.DocStatus, o.CANCELED }).ToListAsync();
            var cANCELEDORDR = oRDRS.Where(o => o.CANCELED == "Y").ToList();
            if (cANCELEDORDR.Count() > 0)
            {
                var cANCELEDORDRIds = cANCELEDORDR.Select(c => c.DocEntry).ToList();
                await UnitWork.UpdateAsync<Quotation>(q => q.QuotationStatus != -1M && cANCELEDORDRIds.Contains((int)q.SalesOrderId), q => new Quotation { QuotationStatus = -1 });
            }
            var statusORDR = oRDRS.Where(o => o.DocStatus == "C").ToList();
            if (statusORDR.Count() > 0)
            {
                var statusORDRIds = statusORDR.Select(c => c.DocEntry).ToList();
                await UnitWork.UpdateAsync<Quotation>(q => q.QuotationStatus != 11M && statusORDRIds.Contains((int)q.SalesOrderId), q => new Quotation { QuotationStatus = 11 });
            }
            await UnitWork.SaveAsync();
        }
        /// <summary>
        /// 客户历史销售订单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> HistorySaleOrde(QueryQuotationListReq request)
        {
            var serviceOrderIds = await UnitWork.Find<ServiceOrder>(null).WhereIf(!string.IsNullOrWhiteSpace(request.ServiceOrderSapId.ToString()),s=>s.U_SAP_ID.ToString().Contains(request.ServiceOrderSapId.ToString())).Where(q => q.TerminalCustomer.Contains(request.CardCode) || q.TerminalCustomerId.Contains(request.CardCode)).Select(s => s.Id).ToListAsync();
            var quotations = UnitWork.Find<Quotation>(q => q.SalesOrderId != null && q.QuotationStatus != -1 && serviceOrderIds.Contains(q.ServiceOrderId))
                .WhereIf(!string.IsNullOrWhiteSpace(request.CreateUser),q=>q.CreateUser.Contains(request.CreateUser))
                .WhereIf(!string.IsNullOrWhiteSpace(request.QuotationId.ToString()), q => q.Id.ToString().Contains(request.QuotationId.ToString()))
                .WhereIf(!string.IsNullOrWhiteSpace(request.StartCreateTime.ToString()), q => q.CreateTime > request.StartCreateTime)
                .WhereIf(!string.IsNullOrWhiteSpace(request.EndCreateTime.ToString()), q => q.CreateTime<Convert.ToDateTime(request.EndCreateTime).AddDays(1));

            var reult = new TableData();
            reult.Data = await quotations.Select(q=>new {
                QuotationId=q.Id,
                q.CreateUser,
                q.CreateTime,
                q.UpDateTime,
                q.TotalMoney,
                q.Remark,
                q.ServiceOrderSapId
            }).Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();
            reult.Count = await quotations.CountAsync();
            return reult;
        }
        /// <summary>
        /// 申请取消销售订单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task CancelRequest(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var quotationObj= await UnitWork.Find<Quotation>(q => q.Id == request.QuotationId && q.CreateUserId.Equals(loginContext.User.Id) && q.QuotationStatus!=11 && q.QuotationStatus != 12).FirstOrDefaultAsync();
            if (quotationObj == null) throw new Exception("暂时不可以申请取消");
            await UnitWork.UpdateAsync<Quotation>(q=>q.Id == request.QuotationId, q=>new Quotation { CancelRequest=1});
            await UnitWork.SaveAsync();
        }

        #region 售后提成
        /// <summary>
        /// 生成售后提成单
        /// </summary>
        /// <returns></returns>
        public async Task GenerateCommissionSettlement()
        {
            var commissionOrder = await UnitWork.Find<CommissionOrder>(c => c.Status == 3).ToListAsync();
            var saleOrderId = commissionOrder.Select(c => c.SalesOrderId).ToList();
            //已出库 物料类型为销售 未生成售后提成
            var query = await (from a in UnitWork.Find<Quotation>(c => !string.IsNullOrWhiteSpace(c.SalesOrderId.ToString()) && c.IsMaterialType == 2 && c.QuotationStatus == 11 && c.CreateTime >= DateTime.Parse("2022-07-11 00:00:00")).Include(c => c.QuotationMergeMaterials).Where(c => c.QuotationMergeMaterials.Any(q => q.MaterialType == 2))
                               join b in UnitWork.Find<CommissionOrder>(null) on a.SalesOrderId equals b.SalesOrderId into ab
                               from b in ab.DefaultIfEmpty()
                               where b == null
                               select new { a.SalesOrderId, a.ServiceOrderId, a.ServiceOrderSapId, a.CreateUser, a.CreateUserId, a.TotalCommission, a.TotalMoney }).ToListAsync();
            List<CommissionOrder> orders = new List<CommissionOrder>();
            query.ForEach(c =>
            {
                orders.Add(new CommissionOrder { SalesOrderId = c.SalesOrderId, Amount = c.TotalCommission, SaleAmout = c.TotalMoney, ServiceOrderId = c.ServiceOrderId, ServiceOrderSapId = c.ServiceOrderSapId, CreateUserId = c.CreateUserId, CreateUser = c.CreateUser, CreateTime = DateTime.Now, UpdateTime = DateTime.Now, Status = 3 });
            });
            await UnitWork.BatchAddAsync<CommissionOrder, int>(orders.ToArray());

            if (saleOrderId.Count > 0)
            {
                //增值税发票
                var billapplication = await UnitWork.Find<finance_billapplication_master>(c => saleOrderId.Contains(c.DocEntry)).Select(c => new { c.DocEntry, c.billStatus }).ToListAsync();
                //收款
                var orderNo = string.Join(",", saleOrderId);
                var orctSql = $@"SELECT * from (
	                                SELECT 
		                                T0.DocTotal,
		                                SE.DocStatus,
		                                SE.DocTotal AS SEDocTotal,
		                                T0.DocEntry,
		                                T0.U_XSDD as OrderNo
	                                FROM
		                                dbo.ORCT AS T0
		                                LEFT OUTER JOIN dbo.ORDR AS SE ON T0.U_XSDD= SE.DocEntry
		                                ) a where OrderNo in ({orderNo})";
                var orct = await UnitWork.Query<ORCTModel>(orctSql).ToListAsync();
                orct = orct.GroupBy(c => c.OrderNo).Select(c => new ORCTModel
                {
                    DocEntry = c.First().DocEntry,
                    DocStatus = c.First().DocStatus,
                    DocTotal = c.Sum(s => s.DocTotal),
                    SEDocTotal = c.First().SEDocTotal,
                    OrderNo = c.Key
                }).ToList();

                var union = (from a in commissionOrder
                             join b in billapplication on a.SalesOrderId equals b.DocEntry into ab
                             from b in ab.DefaultIfEmpty()
                             join c in orct on a.SalesOrderId equals c.OrderNo into ac
                             from c in ac.DefaultIfEmpty()
                             select new
                             {
                                 a.SalesOrderId,
                                 a.CreateUserId,
                                 a.Status,
                                 BillStatus = b == null ? 0 : b.billStatus == 1 || b.billStatus == 3 || b.billStatus == 4 ? 1 : 0,
                                 DocStatus = c == null ? "N" : c.DocStatus == "C" ? "C" : "N",
                                 DocTotal = c == null ? null : c.DocTotal,
                                 SEDocTotal = c == null ? null : c.SEDocTotal
                             }).ToList();
                foreach (var item in union)
                {
                    var receivables = 0;
                    var status = item.Status;
                    if (item.DocTotal != null && item.SEDocTotal != null)
                    {
                        //单据总金额=已收款金额 即为已收齐
                        if (item.SEDocTotal - item.DocTotal == 0 && item.SEDocTotal == item.DocTotal)
                            receivables = 1;
                    }
                    //满足三个条件后转为待处理
                    if (item.DocStatus == "C" && item.BillStatus == 1 && receivables == 1)
                        status = 4;

                    ////创建流程
                    //var mf = _moduleFlowSchemeApp.Get(c => c.Module.Name == "售后提成");
                    //var flow = new AddFlowInstanceReq();
                    //flow.SchemeId = mf.FlowSchemeId;
                    //flow.FrmType = 2;
                    //flow.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                    //flow.CustomName = $"售后提成单";
                    //flow.FrmData = "";
                    //var flowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(flow);

                    await UnitWork.UpdateAsync<CommissionOrder>(c => c.SalesOrderId == item.SalesOrderId, c => new CommissionOrder
                    {
                        DocStatus = item.DocStatus,
                        BillStatus = item.BillStatus,
                        Receivables = receivables,
                        Status = status
                    });
                }
            }
            await UnitWork.SaveAsync();


        }

        /// <summary>
        /// 我的提成 erp app 通用
        /// CommissionReportId= ，PageType == 2 报表下有哪些提成单
        /// PageType == 1 erp 我的提成页面
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> CommissionOrderList(QueryCommissionOrderReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            //var loginUserRole = loginContext.Roles;
            if (loginUser.Account == Define.USERAPP && request.AppUserId > 0)
            {
                loginUser = await GetUserId(Convert.ToInt32(request.AppUserId));
                //loginUserRole = await GetRoleByUserId(loginUser.Id);
            }
            TableData result = new TableData();
            List<int> ServiceOrderIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(request.TerminalCustomer))
            {
                ServiceOrderIds.AddRange(await UnitWork.Find<ServiceOrder>(s => s.TerminalCustomer.Contains(request.TerminalCustomer) || s.TerminalCustomerId.Contains(request.TerminalCustomer)).Select(s => s.Id).ToListAsync());
            }

            var query = UnitWork.Find<CommissionOrder>(null)
                        .WhereIf(!string.IsNullOrWhiteSpace(request.SalesOrderId.ToString()), c => c.SalesOrderId.ToString().Contains(request.SalesOrderId.ToString()))
                        .WhereIf(request.ServiceOrderSapId != null, q => q.ServiceOrderSapId.ToString().Contains(request.ServiceOrderSapId.ToString()))
                        .WhereIf(request.Id != null, q => q.Id==request.Id)
                        .WhereIf(!string.IsNullOrWhiteSpace(request.CreateUser), q => q.CreateUser.Contains(request.CreateUser))
                        .WhereIf(request.StartCreateTime != null, q => q.CreateTime >= request.StartCreateTime)
                        .WhereIf(request.EndCreateTime != null, q => q.CreateTime <= request.EndCreateTime)
                        .WhereIf(request.Status != null, q => q.Status == request.Status)
                        .WhereIf(request.CommissionReportId != null, q => q.CommissionReportId == request.CommissionReportId)
                        .WhereIf(ServiceOrderIds.Count > 0, c => ServiceOrderIds.Contains(c.ServiceOrderId));
            //request.PageType == 1我的提成界面，request.PageType == 2审批中报表下提成单
            if (!loginContext.Roles.Any(r => r.Name.Equals("呼叫中心-查看")) && !loginContext.Roles.Any(r => r.Name.Equals("客服主管")) && !loginUser.Account.Equals(Define.SYSTEM_USERNAME) && request.PageType == 1)
            {
                query = query.Where(q => q.CreateUserId.Equals(loginUser.Id));
            }
            if (request.PageType == 2)
            {
                if (loginContext.Roles.Any(r => r.Name.Equals("财务")))
                {
                    query = query.Where(q => q.ApprovalStatus == 1 || q.ApprovalStatus == 2);
                }
                else if (loginContext.Roles.Any(r => r.Name.Equals("出纳")))
                {
                    query = query.Where(q => q.ApprovalStatus == 3 || q.ApprovalStatus == 4);
                }
            }
            result.Count = await query.CountAsync();
            var queryObj = await query.OrderByDescending(c => c.CreateTime).Skip((request.page - 1) * request.limit).Take(request.limit).ToListAsync();
            var serviceOrderIds = queryObj.Select(c => c.ServiceOrderId).ToList();
            var saleOrderId = queryObj.Select(c => c.SalesOrderId).ToList();
            var quoation = await UnitWork.Find<Quotation>(c => saleOrderId.Contains(c.SalesOrderId)).Select(c => new { c.Id, c.SalesOrderId }).ToListAsync();
            //var completionReports = await UnitWork.Find<CompletionReport>(c => serviceOrderIds.Contains(c.ServiceOrderId.Value)).ToListAsync();
            var serviceOrder = await UnitWork.Find<ServiceOrder>(c => serviceOrderIds.Contains(c.Id)).Include(c => c.ServiceWorkOrders).Select(c => new
            {
                c.Id,
                c.TerminalCustomerId,
                c.TerminalCustomer,
                c.ServiceWorkOrders.FirstOrDefault().FromTheme,
                c.ServiceWorkOrders.FirstOrDefault().MaterialCode,
                c.ServiceWorkOrders.FirstOrDefault().ManufacturerSerialNumber
            }).ToListAsync();
            var userIds = queryObj.Select(c => c.CreateUserId).ToList();
            var SelOrgName = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(null).Select(o => new { o.Id, o.Name, o.CascadeId }).ToListAsync();
            var Relevances = await UnitWork.Find<Relevance>(r => r.Key == Define.USERORG && userIds.Contains(r.FirstId)).Select(r => new { r.FirstId, r.SecondId }).ToListAsync();
            var independentOrg = new string[] { "CS7", "CS12", "CS14", "CS17", "CS20", "CS29", "CS32", "CS34", "CS36", "CS37", "CS38", "CS9", "CS50", "CSYH" };

            var commissionOrder = (from a in queryObj
                                   join b in serviceOrder on a.ServiceOrderId equals b.Id
                                   join c in quoation on a.SalesOrderId equals c.SalesOrderId
                                   orderby a.CreateTime descending
                                   select new
                                   {
                                       a.Id,
                                       QuotationId = c.Id,
                                       a.SalesOrderId,
                                       a.ServiceOrderSapId,
                                       a.Amount,
                                       a.SaleAmout,
                                       a.Remark,
                                       a.CreateTime,
                                       a.PayTime,
                                       a.Status,
                                       a.ApprovalStatus,
                                       b.TerminalCustomerId,
                                       b.TerminalCustomer,
                                       b.ManufacturerSerialNumber,
                                       b.FromTheme,
                                       b.MaterialCode,
                                       a.CreateUserId,
                                       CreateUser = SelOrgName.Where(s => s.Id.Equals(Relevances.Where(w => w.FirstId.Equals(a.CreateUserId)).FirstOrDefault()?.SecondId)).FirstOrDefault()?.Name == null ?
                                       a.CreateUser : SelOrgName.Where(s => s.Id.Equals(Relevances.Where(w => w.FirstId.Equals(a.CreateUserId)).FirstOrDefault()?.SecondId)).FirstOrDefault()?.Name + "-" + a.CreateUser,
                                       IsContracting = independentOrg.Contains(SelOrgName.Where(s => s.Id.Equals(Relevances.Where(w => w.FirstId.Equals(a.CreateUserId)).FirstOrDefault()?.SecondId)).FirstOrDefault()?.Name) ? 1 : 0,

                                   }).ToList();
            result.Data = commissionOrder;
            return result;
        }

        /// <summary>
        /// 待/已处理结算
        /// PageType == 2 app报表单页面及erp已处理
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> CommissionReportList(QueryCommissionOrderReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            //var loginUserRole = loginContext.Roles;
            if (loginUser.Account == Define.USERAPP && request.AppUserId > 0)
            {
                loginUser = await GetUserId(Convert.ToInt32(request.AppUserId));
                //loginUserRole = await GetRoleByUserId(loginUser.Id);
            }
            var query = UnitWork.Find<CommissionReport>(null)
                        .WhereIf(!string.IsNullOrWhiteSpace(request.BatchNo), c => c.BatchNo.Contains(request.BatchNo))
                        .WhereIf(!string.IsNullOrWhiteSpace(request.Org), c => c.OrgName.Contains(request.Org))
                        .WhereIf(!string.IsNullOrWhiteSpace(request.CreateUser), c => c.CreateUser.Contains(request.CreateUser))
                        .WhereIf(request.Status != null, q => q.Status == request.Status)
                        .WhereIf(request.StartCreateTime != null, q => q.CreateTime >= request.StartCreateTime)
                        .WhereIf(request.EndCreateTime != null, q => q.CreateTime <= request.EndCreateTime);

            if (request.PageType == 1)//待处理
            {
                var SchemeContent = await UnitWork.Find<FlowScheme>(f => f.SchemeName.Equals("售后提成单")).Select(f => f.Id).FirstOrDefaultAsync();
                var flowinstace = await UnitWork.Find<FlowInstance>(c => c.SchemeId == SchemeContent && c.MakerList.Contains(loginUser.Id)).Select(c => c.Id).ToListAsync();
                var reportId = await UnitWork.Find<CommissionOrder>(c => flowinstace.Contains(c.FlowInstanceId)).Select(c => c.CommissionReportId).ToListAsync();
                query = query.Where(c => reportId.Contains(c.Id));
            }
            else if (request.PageType == 2)//已处理
            {
                //新建一个报表处理记录表 用于查询已处理数据 *
                //var instances = await UnitWork.Find<FlowInstanceOperationHistory>(c => c.CreateUserId == loginContext.User.Id).Select(c => c.InstanceId).Distinct().ToListAsync();
                var history = await UnitWork.Find<CommissionReportOperationHistory>(c => c.UserId == loginUser.Id).Select(c => c.CommissionReportId).ToListAsync();
                query = query.Where(c => history.Contains(c.Id));
            }

            TableData result = new TableData();

            result.Count = await query.CountAsync();
            var queryObj = await query.OrderByDescending(c => c.CreateTime).Skip((request.page - 1) * request.limit).Take(request.limit).ToListAsync();
            result.Data = queryObj;
            return result;
        }

        /// <summary>
        /// 可提交的提成单
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetAvailableCommissionOrder(QueryCommissionOrderReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP && request.AppUserId > 0)
            {
                loginUser = await GetUserId(Convert.ToInt32(request.AppUserId));
            }
            TableData result = new TableData();
            var obj = UnitWork.Find<CommissionOrder>(c => c.CreateUserId == loginUser.Id && c.Status == 4);
                           
            var query = await obj.OrderByDescending(c => c.CreateTime)
                            //.Skip((request.page - 1) * request.limit)
                            //.Take(request.limit)
                            .ToListAsync();
            result.Count = await obj.CountAsync();
            var serviceOrderId = query.Select(c => c.ServiceOrderId).ToList();
            var serviceOrder = await UnitWork.Find<ServiceOrder>(c => serviceOrderId.Contains(c.Id)).Select(c => new { c.Id, c.TerminalCustomer, c.TerminalCustomerId }).ToListAsync();
            var userIds = query.Select(c => c.CreateUserId).ToList();
            var SelOrgName = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(null).Select(o => new { o.Id, o.Name, o.CascadeId }).ToListAsync();
            var Relevances = await UnitWork.Find<Relevance>(r => r.Key == Define.USERORG && userIds.Contains(r.FirstId)).Select(r => new { r.FirstId, r.SecondId }).ToListAsync();
            result.Data = query.Select(c => new
            {
                TerminalCustomerId = serviceOrder.Where(s => s.Id == c.ServiceOrderId).FirstOrDefault()?.TerminalCustomerId,
                TerminalCustomer = serviceOrder.Where(s => s.Id == c.ServiceOrderId).FirstOrDefault()?.TerminalCustomer,
                //c.CreateUser,
                c.UpdateTime,
                c.CreateTime,
                c.SalesOrderId,
                c.Amount,
                c.SaleAmout,
                c.Remark,
                c.Id,
                CreateUser = SelOrgName.Where(s => s.Id.Equals(Relevances.Where(w => w.FirstId.Equals(c.CreateUserId)).FirstOrDefault()?.SecondId)).FirstOrDefault()?.Name == null ? c.CreateUser : SelOrgName.Where(s => s.Id.Equals(Relevances.Where(w => w.FirstId.Equals(c.CreateUserId)).FirstOrDefault()?.SecondId)).FirstOrDefault()?.Name + "-" + c.CreateUser
            }).ToList();
            return result;
        }

        /// <summary>
        /// 新建报表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> AddReport(AddReportReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP && req.AppUserId > 0)
            {
                loginUser = await GetUserId(Convert.ToInt32(req.AppUserId));
            }
            var loginOrg = await _userManagerApp.GetUserOrgInfo(loginUser.Id);
            Infrastructure.Response response = new Infrastructure.Response();

            var order = await UnitWork.Find<CommissionOrder>(c => req.Ids.Contains(c.Id)).ToListAsync();
            var Message = "";
            order.ForEach(c =>
            {
                if (!string.IsNullOrWhiteSpace(c.CommissionReportId.ToString()))
                {
                    Message += $"单号{c.Id}已提交过报表，请勿重复提交。";
                }
            });
            if (!string.IsNullOrWhiteSpace(Message))
            {
                response.Code = 500;
                response.Message = Message;
                return response;
            }

            var dbContext = UnitWork.GetDbContext<CommissionReport>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var report = await UnitWork.Find<CommissionReport>(null).OrderByDescending(c => c.BatchNo).FirstOrDefaultAsync(); 
                    var year = DateTime.Now.Year.ToString();
                    var num = "01";
                    int temp = 1;
                    if (report == null)
                    {
                        //取3.0批号
                        var fab = await UnitWork.Find<finance_aftersalesbonus_batch>(null).OrderByDescending(c => c.ExportBatchNo).FirstOrDefaultAsync();
                        temp = Convert.ToInt32(fab.ExportBatchNo?.Split(year)[1]);
                        temp++;
                        //num = batchNo.ToString().PadLeft(2, '0');
                    }
                    else
                    {
                        temp = Convert.ToInt32(report.BatchNo?.Split(year)[1]);
                        temp++;
                    }

                    if (temp < 10)
                        num = $"0{temp}";
                    else
                        num = temp.ToString();
                    //批次单号
                    var batchNo = $"SHTC{year}{num}";

                    var orderAmout = order.Sum(c => c.Amount);
                    CommissionReport commissionReport = new CommissionReport();
                    commissionReport.BatchNo = batchNo;
                    commissionReport.OrgId = loginOrg.OrgId;
                    commissionReport.OrgName = loginOrg.OrgName;
                    commissionReport.CreateUserId = loginUser.Id;
                    commissionReport.CreateUser = loginUser.Name;
                    commissionReport.CreateTime = DateTime.Now;
                    commissionReport.UpdateTime = DateTime.Now;
                    commissionReport.Status = 5;
                    commissionReport.Amount = orderAmout;
                    commissionReport = await UnitWork.AddAsync<CommissionReport, int>(commissionReport);
                    await UnitWork.SaveAsync();


                    //await UnitWork.UpdateAsync<CommissionReport>(r => r.Id == commissionReport.Id, r => new CommissionReport { FlowInstanceId = commissionReport.FlowInstanceId });
                    var flowScheme = await UnitWork.Find<FlowScheme>(c => c.SchemeName == "售后提成单").FirstOrDefaultAsync();
                    foreach (var item in order)
                    {
                        //创建流程
                        //var mf = _moduleFlowSchemeApp.Get(c => c.Module.Name == "售后提成");
                        var flow = new AddFlowInstanceReq();
                        flow.SchemeId = flowScheme.Id;
                        flow.FrmType = 2;
                        flow.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                        flow.CustomName = $"售后提成单";
                        flow.FrmData = "";
                        var flowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(flow);

                        await UnitWork.UpdateAsync<CommissionOrder>(r => r.Id == item.Id, r => new CommissionOrder { FlowInstanceId = flowInstanceId, Status = 5, UpdateTime = DateTime.Now, CommissionReportId = commissionReport.Id, ApprovalStatus = 1 });
                    }
                    //报表处理历史
                    await UnitWork.AddAsync<CommissionReportOperationHistory>(new CommissionReportOperationHistory { CommissionReportId = commissionReport.Id, UserId = loginUser.Id, StepName = "待处理" });
                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("添加报表失败。" + ex.Message);
                }
            }
            return response;
        }

        /// <summary>
        /// 审核提成单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task AccraditationCommissionOrder(AccraditationQuotationReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP && req.AppUserId > 0)
            {
                loginUser = await GetUserId(Convert.ToInt32(req.AppUserId));
            }
            var obj = await UnitWork.Find<CommissionOrder>(c => c.Id == req.Id).FirstOrDefaultAsync();
            var orderList = await UnitWork.Find<CommissionOrder>(c => c.CommissionReportId == obj.CommissionReportId).Select(c => new { c.FlowInstanceId, c.Id, c.ApprovalStatus }).ToListAsync();
            var activityName = await UnitWork.Find<FlowInstance>(c => c.Id == obj.FlowInstanceId).Select(c => c.ActivityName).FirstOrDefaultAsync();
            VerificationReq verificationReq = new VerificationReq
            {
                NodeRejectStep = "",
                NodeRejectType = "0",
                FlowInstanceId = obj.FlowInstanceId,
                VerificationFinally = "1",
                VerificationOpinion = req.Remark
            };

            if (req.IsReject)
            {
                verificationReq.VerificationFinally = "3";
                verificationReq.NodeRejectType = "1";

                foreach (var item in orderList)
                {
                    //驳回同一报表下所有提成单
                    verificationReq.FlowInstanceId = item.FlowInstanceId;
                    await _flowInstanceApp.Verification(verificationReq);
                    await UnitWork.UpdateAsync<CommissionOrder>(r => r.Id == item.Id, r => new CommissionOrder { Status = 1, UpdateTime = DateTime.Now, ApprovalStatus = null });
                }
                await UnitWork.UpdateAsync<CommissionReport>(r => r.Id == obj.CommissionReportId, r => new CommissionReport { Status = 1, UpdateTime = DateTime.Now });
                await UnitWork.AddAsync(new CommissionReportOperationHistory { CommissionReportId = obj.CommissionReportId, UserId = loginUser.Id, StepName = "驳回" });
            }
            else
            {
                //var approvalStatus = 99;//审批环节 标识审核人有无审核的状态
                //var status = 5;//单据的整个流程状态

                if (activityName == "财务审批")
                {
                    await UnitWork.UpdateAsync<CommissionOrder>(r => r.Id == obj.Id, r => new CommissionOrder { ApprovalStatus = 2, UpdateTime = DateTime.Now });
                    var approvalCount = orderList.Where(c => c.ApprovalStatus == 2).Count();
                    //报表中其他单都已审批 进入下一审批环节
                    if (orderList.Count - 1 == approvalCount)
                    {
                        foreach (var item in orderList)
                        {
                            verificationReq.FlowInstanceId = item.FlowInstanceId;
                            //都审批通过后才进行真正的审核流程
                            await _flowInstanceApp.Verification(verificationReq);
                        }
                        await UnitWork.UpdateAsync<CommissionOrder>(r => r.CommissionReportId == obj.CommissionReportId, r => new CommissionOrder { Status = 6, ApprovalStatus = 3, UpdateTime = DateTime.Now });
                        await UnitWork.UpdateAsync<CommissionReport>(r => r.Id == obj.CommissionReportId, r => new CommissionReport { Status = 6, UpdateTime = DateTime.Now });
                        await UnitWork.AddAsync(new CommissionReportOperationHistory { CommissionReportId = obj.CommissionReportId, UserId = loginUser.Id, StepName = activityName });
                    }
                }
                else if (activityName == "待结算")
                {
                    await UnitWork.UpdateAsync<CommissionOrder>(r => r.Id == obj.Id, r => new CommissionOrder { ApprovalStatus = 4, UpdateTime = DateTime.Now });
                    var approvalCount = orderList.Where(c => c.ApprovalStatus == 4).Count();
                    //报表中其他单都已审批 进入下一审批环节
                    if (orderList.Count - 1 == approvalCount)
                    {
                        foreach (var item in orderList)
                        {
                            verificationReq.FlowInstanceId = item.FlowInstanceId;
                            //都审批通过后才进行真正的审核流程
                            await _flowInstanceApp.Verification(verificationReq);
                        }
                        await UnitWork.UpdateAsync<CommissionOrder>(r => r.CommissionReportId == obj.CommissionReportId, r => new CommissionOrder { Status = 7, PayTime = DateTime.Now });
                        await UnitWork.UpdateAsync<CommissionReport>(r => r.Id == obj.CommissionReportId, r => new CommissionReport { Status = 7, UpdateTime = DateTime.Now });
                        await UnitWork.AddAsync(new CommissionReportOperationHistory { CommissionReportId = obj.CommissionReportId, UserId = loginUser.Id, StepName = activityName });
                    }
                }

            }
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 批量结算
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task BatchAccraditation(BatchAccraditationReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            foreach (var item in req.Ids)
            {
                var commissionOrder = await UnitWork.Find<CommissionOrder>(c => c.CommissionReportId == item).ToListAsync();
                foreach (var order in commissionOrder)
                {
                    VerificationReq verificationReq = new VerificationReq
                    {
                        NodeRejectStep = "",
                        NodeRejectType = "0",
                        FlowInstanceId = order.FlowInstanceId,
                        VerificationFinally = "1",
                        VerificationOpinion = ""
                    };
                    await _flowInstanceApp.Verification(verificationReq);
                    await UnitWork.UpdateAsync<CommissionOrder>(r => r.Id == order.Id, r => new CommissionOrder { Status = 7, PayTime = req.PayTime, ApprovalStatus = 4 });
                }
                await UnitWork.UpdateAsync<CommissionReport>(r => r.Id == item, r => new CommissionReport { Status = 7, UpdateTime = DateTime.Now, PayTime = req.PayTime });
                await UnitWork.AddAsync(new CommissionReportOperationHistory { CommissionReportId = item, UserId = loginContext.User.Id, StepName = "待结算" });
                await UnitWork.SaveAsync();
            }
        }

        /// <summary>
        /// 撤回报表
        /// </summary>
        /// <returns></returns>
        public async Task RecallReport(AccraditationQuotationReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP && req.AppUserId > 0)
            {
                loginUser = await GetUserId(Convert.ToInt32(req.AppUserId));
            }

            var orderList = await UnitWork.Find<CommissionOrder>(c => c.CommissionReportId == req.Id && c.CreateUserId == loginUser.Id).Select(c => new { c.FlowInstanceId, c.Id }).ToListAsync();
            if (orderList.Count > 0)
            {
                var flowIds = orderList.Select(c => c.FlowInstanceId).ToList();
                var ids = orderList.Select(c => c.Id).ToList();
                await UnitWork.UpdateAsync<CommissionOrder>(r => ids.Contains(r.Id), r => new CommissionOrder { Status = 4, UpdateTime = DateTime.Now, ApprovalStatus = null, FlowInstanceId = "", CommissionReportId = null });
                await UnitWork.DeleteAsync<CommissionReport>(c => c.Id == req.Id);
                await UnitWork.DeleteAsync<CommissionReportOperationHistory>(c => c.CommissionReportId == req.Id);
                await UnitWork.SaveAsync();

                await UnitWork.DeleteAsync<FlowInstance>(c => flowIds.Contains(c.Id));
                await UnitWork.DeleteAsync<FlowInstanceOperationHistory>(c => flowIds.Contains(c.InstanceId));
                await UnitWork.DeleteAsync<FlowInstanceTransitionHistory>(c => flowIds.Contains(c.InstanceId));
                await UnitWork.SaveAsync();
            }
        }

        /// <summary>
        /// 审批详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TableData> GetApproveDetail(int id)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();
            var commissionOrder = await UnitWork.Find<CommissionOrder>(c => c.Id == id).FirstOrDefaultAsync();
            var quaotion = await UnitWork.Find<Quotation>(c => c.SalesOrderId == commissionOrder.SalesOrderId).Select(c => new { c.Id, c.InvoiceCompany, c.SalesOrderId }).FirstOrDefaultAsync();
            //操作历史
            var operationHistories = await UnitWork.Find<FlowInstanceOperationHistory>(c => c.InstanceId == commissionOrder.FlowInstanceId)
                .OrderBy(c => c.CreateDate).Select(h => new
                {
                    CreateTime = Convert.ToDateTime(h.CreateDate).ToString("yyyy.MM.dd HH:mm:ss"),
                    h.Remark,
                    IntervalTime = h.IntervalTime != null && h.IntervalTime > 0 ? h.IntervalTime / 60 : null,
                    h.CreateUserName,
                    h.Content,
                    h.ApprovalResult,
                }).ToListAsync();
            //销售单
            var ordr = await UnitWork.Find<ORDR>(c => c.DocEntry == commissionOrder.SalesOrderId).Select(c => new { c.CardCode, c.CardName, c.CntctCode, c.Indicator, c.U_FPLB, c.U_SL, c.ShipToCode, c.PayToCode, c.Address, c.DocDate, c.DocDueDate, c.Comments, c.GroupNum, c.DocCur, c.DocRate, c.TotalExpns, c.DocType, WhsCode = "01", c.PartSupply }).FirstOrDefaultAsync();
            //科目余额和比率
            var balance = await AccountBalance(ordr.CardCode);
            var margeMatrials = await UnitWork.Find<QuotationMergeMaterial>(c => c.QuotationId == quaotion.Id).ToListAsync();
            //增值税发票Id
            var finance_billapplication_master = await UnitWork.Find<finance_billapplication_master>(c => c.DocEntry == commissionOrder.SalesOrderId).Select(c => c.billID).ToListAsync();
            //测试 需注释
            //finance_billapplication_master = await UnitWork.Find<finance_billapplication_master>(c => c.DocEntry == 55859).Select(c => c.billID).ToListAsync();
            //发票附件
            var invoiceFile = await (from a in UnitWork.Find<file_main>(null)
                                     join b in UnitWork.Find<file_type>(null) on a.file_type_id equals b.type_id
                                     where finance_billapplication_master.Contains(a.docEntry) && a.file_type_id == 35 && a.sbo_id == 0
                                     select new { a.file_id, b.type_nm, a.file_nm, a.file_path }).ToListAsync();
            //var category = await UnitWork.Find<Category>(c => c.TypeId == "SYS_InvoiceCompany" && c.DtValue == quaotion.InvoiceCompany).FirstOrDefaultAsync();
            var contractFile = await PrintSalesOrder(quaotion.SalesOrderId.Value);//合同附件

            List<FileResp> files = new List<FileResp>();
            invoiceFile.ForEach(c =>
            {
                files.Add(new FileResp { FileName = c.file_nm, FileType = c.type_nm, FilePath = $"{_appConfiguration.Value.ERP3Url}{c.file_path}".Replace("192.168.0.208", "218.17.149.195") });
            });
            files.Add(new FileResp { FileName = "销售合同.pdf", FileType = "销售合同", FilePath = contractFile });
            files.Add(new FileResp { FileName = "装箱发货清单.pdf", FileType = "装箱发货清单", FilePath = quaotion.Id.ToString() });
            result.Data = new
            {
                commissionOrder.Id,
                TotalCommission = commissionOrder.Amount,
                TotalMoney = commissionOrder.SaleAmout,
                Balance = balance.Balance,
                Rate = balance.Rate,
                SaleOrder = ordr,
                MergeMaterial = margeMatrials.Select(c => new
                {
                    c.MaterialType,
                    c.MaterialCode,
                    c.MaterialDescription,
                    c.SentQuantity,
                    c.CostPrice,
                    c.DiscountPrices,
                    c.TotalPrice,
                    HLC = 0,
                    c.Commission
                }),
                Files = files,
                OperationHistory = operationHistories
            };
            return result;
        }

        public async Task<TableData> DropDownOptions()
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();
            //标识
            var crm_oidc = await UnitWork.Find<crm_oidc>(c => c.sbo_id == Define.SBO_ID).Select(c => new { Id = c.Code, c.Name }).ToListAsync();
            //付款条件
            var crm_octg = await UnitWork.Find<crm_octg>(c => c.sbo_id == Define.SBO_ID).Select(c => new { Id = c.GroupNum, Name = c.PymntGroup }).ToListAsync();
            //币种
            var crm_ocrn =await UnitWork.Find<crm_ocrn>(c => c.sbo_id == Define.SBO_ID).Select(c => new { Id = c.CurrCode, Name = c.CurrName }).ToListAsync();

            result.Data = new
            {
                crm_oidc,
                crm_octg,
                crm_ocrn
            };
            return result;
        }

        /// <summary>
        /// 科目余额和比率
        /// </summary>
        /// <param name="cardcode"></param>
        /// <returns></returns>
        public async Task<(decimal Balance, string Rate)> AccountBalance(string cardcode)
        {
            var sql = @$"SELECT (Select sum(Balance) from OCRD where CardCode='{cardcode}') as Balance
                    ,(select sum(DocTotal) from OINV WHERE CANCELED ='N' and CardCode='{cardcode}') as INVtotal
                    ,(select SUM(DocTOTal) from ORIN where CANCELED<>'Y' and CardCode='{cardcode}') as RINtotal
                    --90天内未清收款
                    ,(select SUM(openBal) from ORCT WHERE CANCELED='N' AND openBal<>0 AND CardCode='{cardcode}' and datediff(DAY,docdate,getdate())<=90) as RCTBal90
                    --90天内未清发票金额
                    ,(select SUM(DocTotal-PaidToDate) from OINV WHERE CANCELED ='N' and CardCode='{cardcode}' and DocTotal-PaidToDate>0 and datediff(DAY,docdate,getdate())<=90) as INVBal90
                    --90天内未清贷项金额
                    ,(select SUM(DocTotal-PaidToDate) from ORIN where CANCELED ='N' and CardCode='{cardcode}' and DocTotal-PaidToDate>0 and datediff(DAY,docdate,getdate())<=90) as RINBal90
                    --90天前未清发票的发票总额
                    ,(select SUM(DocTotal) from OINV WHERE CANCELED ='N' and CardCode = '{cardcode}' and DocTotal-PaidToDate > 0 and datediff(DAY, docdate, getdate())> 90) as INVTotal90P";
            var query = await UnitWork.Query<BalanceModel>(sql).FirstOrDefaultAsync();
            query.INVTotal90P = query.INVTotal90P ?? 0;
            var rate = "0.00%";
            if (query.INVTotal90P > 0)
            {
                //=（科目余额+90天内未清收款-90天内未清发票+90天内未清贷项凭证) / 90天以前未清发票的发票总额
                decimal cal = (((query.Balance ?? 0 + query.RCTBal90 ?? 0 - query.INVBal90 ?? 0 + query.RINBal90 ?? 0) / query.INVTotal90P)*100).Value;
                rate =Math.Round(cal,2).ToString() + "%";
            }
            return (query.Balance.Value, rate);
        }

        /// <summary>
        /// 查看附件
        /// </summary>
        /// <param name="saleOrderId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<TableData> ViewAttachment(int saleOrderId, int type)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();
            result.Data = "";
            if (type == 1)//销售合同
            {
                result.Data = PrintSalesOrder(saleOrderId);//合同附件
            }
            else if (type == 2)//发票
            {
                //增值税发票Id
                var finance_billapplication_master = await UnitWork.Find<finance_billapplication_master>(c => c.DocEntry == saleOrderId).Select(c => c.billID).ToListAsync();
                //发票附件
                var invoiceFile = await (from a in UnitWork.Find<file_main>(null)
                                         join b in UnitWork.Find<file_type>(null) on a.file_type_id equals b.type_id
                                         where finance_billapplication_master.Contains(a.docEntry) && a.file_type_id == 35 && a.sbo_id == 0
                                         select new { a.file_id, b.type_nm, a.file_nm, a.file_path }).FirstOrDefaultAsync();
                if (invoiceFile != null && !string.IsNullOrWhiteSpace(invoiceFile.file_path))
                {
                    result.Data = $"{_appConfiguration.Value.ERP3Url}{invoiceFile.file_path}".Replace("192.168.0.208", "218.17.149.195");
                }
            }
            return result;

        }

        /// <summary>
        /// 提成单详情 app用
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> CommissionOrderDetail(QueryCommissionOrderReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            //var loginUserRole = loginContext.Roles;
            if (loginUser.Account == Define.USERAPP && request.AppUserId > 0)
            {
                loginUser = await GetUserId(Convert.ToInt32(request.AppUserId));
                //loginUserRole = await GetRoleByUserId(loginUser.Id);
            }
            TableData result = new TableData();
            var quaotion = await UnitWork.Find<Quotation>(c => c.SalesOrderId == request.SalesOrderId).FirstOrDefaultAsync();
            var margeMatrials = await UnitWork.Find<QuotationMergeMaterial>(c => c.QuotationId == quaotion.Id).ToListAsync();
            result.Data = margeMatrials;
            return result;
        }

        /// <summary>
        /// 打印提成报表
        /// </summary>
        /// <param name="CommissionReportId"></param>
        /// <returns></returns>
        public async Task<byte[]> PrintCommissionReport(string CommissionReportId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var commissionReport = await UnitWork.Find<CommissionReport>(c => c.Id == int.Parse(CommissionReportId)).FirstOrDefaultAsync();
            var commissionOrder = await UnitWork.Find<CommissionOrder>(c => c.CommissionReportId == int.Parse(CommissionReportId)).ToListAsync();
            var userinfo =await _userManagerApp.GetUserOrgInfo(commissionReport.CreateUserId);
            var saleOrderIds = commissionOrder.Select(c => c.SalesOrderId).ToList();
            var serviceOrderIds= commissionOrder.Select(c => c.ServiceOrderId).ToList();
            var serviceOrder = await UnitWork.Find<ServiceOrder>(c => serviceOrderIds.Contains(c.Id)).Select(c => new { c.Id, c.TerminalCustomerId }).ToListAsync();
            var quotation = await UnitWork.Find<Quotation>(c => saleOrderIds.Contains(c.SalesOrderId)).Include(c => c.QuotationMergeMaterials).ToListAsync();
            var cost = quotation.Select(c => new
            {
                c.SalesOrderId,
                CostPrice = c.QuotationMergeMaterials.Sum(q => q.CostPrice * q.Count)
            });
            var model = from a in commissionOrder
                        join b in cost on a.SalesOrderId equals b.SalesOrderId
                        join c in serviceOrder on a.ServiceOrderId equals c.Id
                        select new { a.Id, a.CreateUser, c.TerminalCustomerId, a.SalesOrderId, a.SaleAmout, b.CostPrice, a.Amount, Rate = a.Amount / a.SaleAmout * 100 };

            var url = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "CommissionReportHeader.html");
            var text = System.IO.File.ReadAllText(url);
            text = text.Replace("@Model.Name", $"{userinfo.OrgName}{userinfo.Name}");
            text = text.Replace("@Model.BatchNo", commissionReport.BatchNo);
            var tempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"CommissionReportHeader{commissionReport.Id}.html");
            System.IO.File.WriteAllText(tempUrl, text, Encoding.Unicode);

            var datas = await ExportAllHandler.Exporterpdf(model, "CommissionReport.cshtml", pdf =>
            {
                pdf.IsWriteHtml = true;
                pdf.PaperKind = PaperKind.A4;
                pdf.Orientation = Orientation.Portrait;
                pdf.HeaderSettings = new HeaderSettings() { HtmUrl = tempUrl };
                //pdf.FooterSettings = new FooterSettings() { HtmUrl = footerUrl };
            });
            System.IO.File.Delete(tempUrl);
            //System.IO.File.Delete(footerUrl);
            return datas;
        }
        /// <summary>
        /// 销售收款明细
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TableData> GetOrderPaymentAndFee(int? id)
        { 
            TableData result = new TableData();
            var commissionOrder = await UnitWork.Find<CommissionOrder>(c => c.Id == id).FirstOrDefaultAsync();
            if (commissionOrder != null)
            {
                var sql = $@"SELECT * from (
                    select '1' as DocType,DocEntry,SaleNo as OrderNo,DocTotal,DocDate,Printed,PaymentMethod,PaymentAcct,'' as SettleType from
                    (SELECT T0.DocEntry,T3.DocEntry AS SaleNo,T0.TransId,T0.DocTotal,T0.DocDate,T0.DocDueDate,T0.Printed
                    ,CASE when t0.CashSum>0 then 0 else 1 end as paymentMethod
                    ,CASE when t0.CashSum > 0 then (select AcctCode + ' ' + AcctName from OACT where AcctCode = t0.CashAcct) else (select AcctCode + ' ' + AcctName from OACT where AcctCode = t0.TrsfrAcct) end as PaymentAcct
                    FROM ORCT AS T0 
                    LEFT JOIN RCT2 AS T4 ON T0.DocEntry = T4.DocNum and T4.InvType=13 
                    LEFT JOIN INV1 AS T1 ON T4.DocEntry = T1.DocEntry 
                    LEFT JOIN DLN1 AS T2 ON T1.BaseEntry = T2.DocEntry AND T1.BaseLine = T2.LineNum AND T1.BaseType = 15 
                    LEFT JOIN RDR1 AS T3 ON T2.BaseEntry = T3.DocEntry AND T2.BaseLine = T3.LineNum AND T2.BaseType = 17  
                    where T0.Canceled='N' and T3.DocEntry='{commissionOrder.SalesOrderId}' and T0.U_BonusOrderNo IS NULL
                    group by T0.DocEntry,T3.DocEntry,T0.TransId,T0.DocTotal,T0.DocDate,T0.DocDueDate,t0.CashSum,t0.CashAcct,t0.TrsfrAcct,T0.Printed) T  
                    where SaleNo is not null and SaleNo='{commissionOrder.SalesOrderId}' 
                    union 
                    select 1 as DocType,Tk.DocEntry,Tk.U_XSDD as SaleNo,Tk.DocTotal,Tk.DocDate,Tk.Printed,CASE when Tk.CashSum>0 then 0 else 1 end as paymentMethod
                    ,CASE when Tk.CashSum > 0 then (select AcctCode + ' ' + AcctName from OACT where AcctCode = Tk.CashAcct) 
                    else (select AcctCode + ' ' + AcctName from OACT where AcctCode = Tk.TrsfrAcct) end as PaymentAcct,'' as SettleType
                    from ORCT Tk where Tk.Canceled='N' and Tk.U_XSDD='{commissionOrder.SalesOrderId}' and Tk.U_BonusOrderNo IS NULL
                    ) a";

                var orct = UnitWork.ExcuteSql<ORCTDto>(ContextType.SapDbContextType, sql, CommandType.Text, null);

                var orderDeli = string.Format(@"SELECT r.DocEntry,r.DocTotal,r.U_DocRCTAmount as PayAmt,d.DeliAmt,d.DeliDiscAmt,f.FeeAmt,r.U_BonusSetType,d.DeliAmtFC,d.DeliDiscAmtFC
                from {0}.sale_ordr r
                left outer join ( select {1} as DocEntry,{2} as sbo_id ,sum(d0.docTotal) as DeliAmt,sum(d0.DiscSum) as DeliDiscAmt,sum(d0.DocTotalFC) as DeliAmtFC,sum(d0.DiscSumFC) as DeliDiscAmtFC from {0}.sale_odln d0 
                    where d0.CANCELED='N' and d0.sbo_id={2} and exists( select 1 from {0}.sale_dln1 d1 where d1.DocEntry=d0.DocEntry and d1.sbo_id=d0.sbo_id and d1.basetype=17 and d1.baseentry={1})
                ) d on d.docentry=r.docentry and d.sbo_id=r.sbo_id
                left outer join ( 
                    select {1} as DocEntry,{2} as sbo_id ,sum(FeeAmount) as FeeAmt from {0}.sale_ordr_PLFee where CANCELED='N' and OrderNo={1} and SboId={2}
                ) f on f.docentry=r.docentry and f.sbo_id=r.sbo_id
                where r.docentry={1} and r.sbo_id={2}", "nsap_bone", commissionOrder.SalesOrderId, 1);

                var payment = UnitWork.ExcuteSql<PayMentDto>(ContextType.NsapBoneDbContextType, orderDeli, CommandType.Text, null).FirstOrDefault();
                result.Data = new
                {
                    List = orct,
                    Detail = payment
                };
            }
            return result;
        }

        #endregion

        public async Task<List<SysEquipmentColumn>> GetMaterialDetial(int serviceOrderId ,List<string> MaterialCode)
        {
            //1.判断内部关联单是否存在
            //2.取出变更内容
            //3.根据变更后物料查询出基本信息
            //4.如果服务单存在多个物料编码则进行物料关联
            List<SysEquipmentColumn> result = new List<SysEquipmentColumn>();
            try
            {
                var internalContact = await UnitWork.Find<InternalContactServiceOrder>(null).Where(a => a.ServiceOrderId == serviceOrderId).FirstOrDefaultAsync();
                if (internalContact != null)
                {
                    var info = await UnitWork.Find<InternalContact>(null).Where(a => a.Id == internalContact.InternalContactId).FirstOrDefaultAsync();

                    if (info != null)
                    {
                        var contentJson = JsonHelper.Instance.Deserialize<List<ContentJson>>(info.Content);

                        var content = contentJson.Select(a => new { a.postMaterial, a.materialCode }).ToList();
                        var postList = contentJson.Select(a => a.postMaterial).ToList();

                        var listMaterial = from a in UnitWork.Find<OITM>(null).Where(q => postList.Contains(q.ItemCode))
                                           join b in UnitWork.Find<OITW>(null) on a.ItemCode equals b.ItemCode into ab
                                           from b in ab.DefaultIfEmpty()
                                           where b.WhsCode == "37"
                                           select new SysEquipmentColumn { ItemCode = a.ItemCode, ItemName = a.ItemName, lastPurPrc = a.LastPurPrc, BuyUnitMsr = a.SalUnitMsr, OnHand = b.OnHand, WhsCode = b.WhsCode };
                        var filterCode = await UnitWork.Find<OITM>(c => c.ItemCode.StartsWith("A6") && (c.ItemName.Contains("机柜") || c.ItemName.Contains("机箱"))).Select(c => c.ItemCode).ToListAsync();
                        listMaterial = listMaterial.Where(c => !filterCode.Contains(c.ItemCode));
                        var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ShieldingMaterials")).Select(u => u.Name).ToListAsync();
                        listMaterial = listMaterial.Where(e => !CategoryList.Contains(e.ItemCode));

                        var materialDetial = await listMaterial.ToListAsync();

                        var ItemCodes = materialDetial.Select(e => e.ItemCode).ToList();
                        var MaterialPrices = await UnitWork.Find<MaterialPrice>(m => ItemCodes.Contains(m.MaterialCode)).ToListAsync();

                        List<SysEquipmentColumn> list = new List<SysEquipmentColumn>();

                        Dictionary<string, string> dic = new Dictionary<string, string>();

                        if (MaterialCode.Count > 1)
                        {
                            var code = contentJson.Select(a => a.materialCode).Distinct().ToList();
                            foreach (var item in code)
                            {
                                var association = await GetAssociation(item, MaterialCode);
                                dic.Add(item, association);
                            }
                        }
                        List<SysEquipmentColumn> resultMaterial = new List<SysEquipmentColumn>();

                        foreach (var item in contentJson)
                        {
                            var material = materialDetial.FirstOrDefault(a => a.ItemCode == item.postMaterial);
                            if (material == null)
                            {
                                continue;
                            }
                            material.Quantity = Convert.ToInt32(item.postNums);



                            var Prices = MaterialPrices.Where(m => m.MaterialCode.Equals(material.ItemCode)).FirstOrDefault();
                            //4.0存在物料价格，取4.0的价格为售后结算价，不存在就当前进货价*1.2 为售后结算价。销售价均为售后结算价*3
                            if (Prices != null)
                            {
                                material.UnitPrice = Prices?.SettlementPrice == null || Prices?.SettlementPrice <= 0 ? material.lastPurPrc * Prices?.SettlementPriceModel : Prices?.SettlementPrice;

                                material.UnitPrice = decimal.Parse(material.UnitPrice.ToString("#0.0000"));
                                material.lastPurPrc = material.UnitPrice * Prices?.SalesMultiple;
                            }
                            else
                            {
                                material.UnitPrice = material.lastPurPrc * 1.2M;

                                material.UnitPrice = decimal.Parse(material.UnitPrice.ToString("#0.0000"));
                                material.lastPurPrc = material.UnitPrice * 3;
                            }
                            if (MaterialCode.Count > 1)
                            {
                                material.MnfSerial = dic[item.materialCode];

                                var mnfSerialList = material.MnfSerial.Split(",");
                                if (mnfSerialList.Count() > 1)
                                {
                                    material.MnfSerial = mnfSerialList[0];
                                    for (int i = 1; i <= mnfSerialList.Count(); i++)
                                    {
                                        var NewMaterial = JsonHelper.Instance.Deserialize<SysEquipmentColumn>(JsonHelper.Instance.Serialize(material));
                                        NewMaterial.MnfSerial = mnfSerialList[i];
                                        result.Add(NewMaterial);

                                    }
                                }
                            }
                            else
                            {
                                material.MnfSerial = MaterialCode[0];
                            }

                            resultMaterial.Add(material);

                        }
                        result = resultMaterial.GroupBy(a => new { a.ItemCode, a.MnfSerial }).Select(a => new SysEquipmentColumn
                        {
                            ItemCode = a.Key.ItemCode,
                            MnfSerial = a.Key.MnfSerial,
                            ItemName = a.Max(a => a.ItemName),
                            BaseEntry = a.Max(a => a.BaseEntry),
                            BuyUnitMsr = a.Max(a => a.BuyUnitMsr),
                            Commission = a.Max(a => a.Commission),
                            DocEntry = a.Max(a => a.DocEntry),
                            lastPurPrc = a.Max(a => a.lastPurPrc),
                            OnHand = a.Max(a => a.OnHand),
                            WhsCode = a.Max(a => a.WhsCode),
                            UnitPrice = a.Max(a => a.UnitPrice),
                            Quantity = a.Sum(a => a.Quantity),

                        }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public async Task<string> GetAssociation(string ItemCode, List<string> MaterialCode)
        {
            var material = await UnitWork.Find<InternalContactMaterial>(m => m.MaterialCode == ItemCode).FirstOrDefaultAsync();  
            List<string> itemcode = new List<string>();
            #region 匹配物料编码
            var regex = new List<string>();
            string where = "";
            //
            var Prefix = material.Prefix.Split(",");
            Prefix.ForEach(c =>
            {
                regex.Add($"^{c}-[{string.Join("|", material.Series)}][0-999].*");
            });
            regex.ForEach(c =>
            {
                if (!string.IsNullOrWhiteSpace(where))
                {
                    where += " or ";
                }
                where += $"ItemCode REGEXP '{c}'";
            });
            var sql = $"SELECT * from materialrange where ({where}) and (Volt>={material.VoltsStart} and Volt<={material.VoltseEnd}) and (Amp>={material.AmpsStart} and Amp<={material.AmpsEnd} and Unit='{material.CurrentUnit}')";
            var queryItemCode = UnitWork.Query<MaterialRange>(sql).ToList();

            var Fixture = material.Fixture.Split(",").ToList();
            var Special = material.Special.Split(",").ToList();
            if ((material.Fixture == null || Fixture.Count == 0) && (material.Special == null || Special.Count == 0))
            {
                //没有后缀条件 筛选量程范围内所有物料
                itemcode.AddRange(queryItemCode.Select(c => c.ItemCode).ToList());
            }
            else
            {
                //有后缀条件 按条件排列组合
                queryItemCode = queryItemCode.GroupBy(c => string.Join("-", c.ItemCode.Split("-").Take(3))).Select(c => c.First()).ToList();
                queryItemCode.ForEach(c =>
                {
                    //var am = $"{c.Prefix}-{c.Volt}V{c.Amp}{c.Unit}";
                    var am = string.Join("-", c.ItemCode.Split('-').Take(3));
                    //夹具
                    if (Fixture != null && Fixture.Count > 0)
                    {
                        Fixture.ForEach(fi =>
                        {
                            var ft = $"{am}-{fi}";
                            itemcode.Add(ft);
                            //后缀
                            if (material.Special != null && Special.Count > 0)
                            {
                                material.Special.ForEach(sp =>
                                {
                                    var s = $"{ft}{sp}";
                                    itemcode.Add(s);
                                });
                            }
                            else
                            {
                                itemcode.Add(am);
                            }
                        });
                    }
                    else
                    {
                        //后缀
                        if (material.Special != null && Special.Count > 0)
                        {
                            material.Special.ForEach(sp =>
                            {
                                var s = $"{am}{sp}";
                                itemcode.Add(s);
                            });
                        }
                        else
                        {
                            itemcode.Add(am);
                        }
                    }
                });
            }
            #endregion
            var result = string.Join(",", itemcode.Intersect(MaterialCode).ToArray());
            return result;
        }

        public QuotationApp(IUnitWork unitWork, ICapPublisher capBus, FlowInstanceApp flowInstanceApp, WorkbenchApp workbenchApp, 
            ModuleFlowSchemeApp moduleFlowSchemeApp, IOptions<AppSetting> appConfiguration, IAuth auth, OrgManagerApp orgApp,
            UserManagerApp userManagerApp) : base(unitWork, auth)
        {
            _appConfiguration = appConfiguration;
            _flowInstanceApp = flowInstanceApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
            _capBus = capBus;
            _workbenchApp = workbenchApp;
            _orgApp = orgApp;
            _userManagerApp = userManagerApp;
        }

    }
}
