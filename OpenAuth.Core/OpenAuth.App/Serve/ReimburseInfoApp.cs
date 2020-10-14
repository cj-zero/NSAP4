using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Export;
using Infrastructure.Extensions;
using Infrastructure.GeneralAnalytical;
using log4net.Appender;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npoi.Mapper;
using NStandard;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Serve.Request;
using OpenAuth.App.Serve.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using static Infrastructure.GeneralAnalytical.ConvertTheType;

namespace OpenAuth.App
{
    public class ReimburseInfoApp : OnlyUnitWorkBaeApp
    {
        private RevelanceManagerApp _revelanceApp;
        private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;
        private readonly FlowInstanceApp _flowInstanceApp;
        static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);//用信号量代替锁

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
                OrgUserIds.AddRange(await UnitWork.Find<Relevance>(r => orgids.Contains(r.SecondId) && r.Key == "UserOrg").Select(r => r.FirstId).ToListAsync());
            }

            var result = new TableData();
            var objs = UnitWork.Find<ReimburseInfo>(null);
            var ReimburseInfos = objs.WhereIf(!string.IsNullOrWhiteSpace(request.MainId), r => r.MainId.ToString().Contains(request.MainId))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.ServiceOrderId), r => r.ServiceOrderSapId.ToString().Contains(request.ServiceOrderId))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.BearToPay), r => r.BearToPay.Contains(request.BearToPay))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.Responsibility), r => r.Responsibility.Contains(request.Responsibility))
                      .WhereIf(request.StaticDate != null && request.EndDate != null, r => r.CreateTime > request.StaticDate && r.CreateTime < Convert.ToDateTime(request.EndDate).AddMinutes(1440))
                      //.WhereIf(!string.IsNullOrWhiteSpace(request.IsDraft.ToString()), r => r.IsDraft == request.IsDraft)
                      .WhereIf(!string.IsNullOrWhiteSpace(request.ReimburseType), r => r.ReimburseType.Equals(request.ReimburseType))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.CreateUserName), r => UserIds.Contains(r.CreateUserId))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.OrgName), r => OrgUserIds.Contains(r.CreateUserId))
                      .WhereIf(!string.IsNullOrWhiteSpace(request.TerminalCustomer), r => ServiceOrderIds.Contains(r.ServiceOrderId));

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
            if (request.PageType == 1 && !loginContext.Roles.Any(r => r.Name.Equals("客服主管")) && loginContext.User.Account != "NsapSystem")
            {
                ReimburseInfos = ReimburseInfos.Where(r => r.CreateUserId.Equals(loginContext.User.Id));
            };
            #endregion

            ReimburseInfos = ReimburseInfos.OrderByDescending(u => u.CreateTime);

            #region 页面选择
            switch (request.PageType)
            {
                case 2:
                    if (loginContext.Roles.Any(r => r.Name.Equals("客服主管")))
                    {
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 4);
                    }
                    else if (loginContext.Roles.Any(r => r.Name.Equals("财务初审")))
                    {
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 5);
                    }
                    else if (loginContext.Roles.Any(r => r.Name.Equals("财务复审")))
                    {
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 6);
                    }
                    else if (loginContext.Roles.Any(r => r.Name.Equals("总经理")))
                    {
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 7);
                    }
                    else
                    {
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 0);
                    }
                    ReimburseInfos = ReimburseInfos.OrderBy(u => u.CreateTime);
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
                    if (loginContext.Roles.Any(r => r.Name.Equals("出纳")))
                    {
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 8);
                    }
                    else
                    {
                        ReimburseInfos = ReimburseInfos.Where(r => r.RemburseStatus == 0);
                    }
                    ReimburseInfos = ReimburseInfos.OrderBy(u => u.CreateTime);
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

            var SelOrgName = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(null).Select(o => new { o.Id, o.Name, o.CascadeId }).ToListAsync();
            var SelUserName = await UnitWork.Find<User>(null).Select(u => new { u.Id, u.Name }).ToListAsync();
            var Relevances = await UnitWork.Find<Relevance>(r => r.Key == "UserOrg").Select(r => new { r.FirstId, r.SecondId }).ToListAsync();

            var datas = await ReimburseInfos
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();
            ServiceOrderIds = datas.Select(d => d.ServiceOrderId).ToList();
            var CompletionReports = await UnitWork.Find<CompletionReport>(c => ServiceOrderIds.Contains((int)c.ServiceOrderId)).ToListAsync();
            var ServiceOrders = await UnitWork.Find<ServiceOrder>(s => ServiceOrderIds.Contains(s.Id)).ToListAsync();
            result.Data = datas.Select(d => new
            {
                ReimburseResp = d,
                FillTime = d.CreateTime.ToString("yyyy-MM-dd"),
                OrgName = SelOrgName.Where(o => o.Id.Equals(Relevances.Where(r => r.FirstId.Equals(d.CreateUserId)).Select(r => r.SecondId).FirstOrDefault())).OrderBy(o => o.CascadeId).Select(o => o.Name).FirstOrDefault(),
                UserName = SelUserName.Where(u => u.Id.Equals(d.CreateUserId)).Select(u => u.Name).FirstOrDefault(),
                TerminalCustomerId = CompletionReports.Where(c => c.ServiceOrderId == d.ServiceOrderId && c.CreateUserId.Equals(d.CreateUserId)).Select(c => c.TerminalCustomerId).FirstOrDefault(),
                TerminalCustomer = CompletionReports.Where(c => c.ServiceOrderId == d.ServiceOrderId && c.CreateUserId.Equals(d.CreateUserId)).Select(c => c.TerminalCustomer).FirstOrDefault(),
                BusinessTripDate = CompletionReports.Where(c => c.ServiceOrderId == d.ServiceOrderId && c.CreateUserId.Equals(d.CreateUserId)).Select(c => c.BusinessTripDate).FirstOrDefault().ToDateTime().ToString("yyyy-MM-dd"),
                EndDate = CompletionReports.Where(c => c.ServiceOrderId == d.ServiceOrderId && c.CreateUserId.Equals(d.CreateUserId)).Select(c => c.EndDate).FirstOrDefault().ToDateTime().ToString("yyyy-MM-dd"),
                BusinessTripDays = CompletionReports.Where(c => c.ServiceOrderId == d.ServiceOrderId && c.CreateUserId.Equals(d.CreateUserId)).Select(c => c.BusinessTripDays).FirstOrDefault(),
                FromTheme = CompletionReports.Where(c => c.ServiceOrderId == d.ServiceOrderId && c.CreateUserId.Equals(d.CreateUserId)).Select(c => c.FromTheme).FirstOrDefault(),
                SalesMan = ServiceOrders.Where(s => s.Id == d.ServiceOrderId).Select(s => s.SalesMan).FirstOrDefault()
            });

            result.Count = ReimburseInfos.Count();
            return result;
        }

        /// <summary>
        /// App加载列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> AppLoad(QueryReimburseInfoListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == "App")
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

            var SelOrgName = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(null).Select(o => new { o.Id, o.Name, o.CascadeId }).ToListAsync();
            var SelUserName = await UnitWork.Find<User>(null).Select(u => new { u.Id, u.Name }).ToListAsync();
            var Relevances = await UnitWork.Find<Relevance>(r => r.Key == "UserOrg").Select(r => new { r.FirstId, r.SecondId }).ToListAsync();
            var rohs = await UnitWork.Find<ReimurseOperationHistory>(r => r.ApprovalResult == "驳回").ToListAsync();

            var datas = await ReimburseInfos.OrderByDescending(u => u.CreateTime)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();
            var ServiceOrderIds = datas.Select(d => d.ServiceOrderId).ToList();
            var CompletionReports = await UnitWork.Find<CompletionReport>(c => ServiceOrderIds.Contains((int)c.ServiceOrderId)).ToListAsync();
            var ServiceOrders = await UnitWork.Find<ServiceOrder>(s => ServiceOrderIds.Contains(s.Id)).ToListAsync();
            result.Data = datas.Select(d => new
            {
                ReimburseResp = d,
                RejectRemark = rohs.Where(r => r.ReimburseInfoId == d.Id).Select(r => r.Remark).FirstOrDefault(),
                OrgName = SelOrgName.Where(o => o.Id.Equals(Relevances.Where(r => r.FirstId.Equals(d.CreateUserId)).Select(r => r.SecondId).FirstOrDefault())).OrderBy(o => o.CascadeId).Select(o => o.Name).FirstOrDefault(),
                UserName = SelUserName.Where(u => u.Id.Equals(d.CreateUserId)).Select(u => u.Name).FirstOrDefault(),
                TerminalCustomerId = CompletionReports.Where(c => c.ServiceOrderId == d.ServiceOrderId && c.CreateUserId.Equals(d.CreateUserId)).Select(c => c.TerminalCustomerId).FirstOrDefault(),
                TerminalCustomer = CompletionReports.Where(c => c.ServiceOrderId == d.ServiceOrderId && c.CreateUserId.Equals(d.CreateUserId)).Select(c => c.TerminalCustomer).FirstOrDefault(),
                BusinessTripDate = CompletionReports.Where(c => c.ServiceOrderId == d.ServiceOrderId && c.CreateUserId.Equals(d.CreateUserId)).Select(c => c.BusinessTripDate).FirstOrDefault(),
                EndDate = CompletionReports.Where(c => c.ServiceOrderId == d.ServiceOrderId && c.CreateUserId.Equals(d.CreateUserId)).Select(c => c.EndDate).FirstOrDefault(),
                BusinessTripDays = CompletionReports.Where(c => c.ServiceOrderId == d.ServiceOrderId && c.CreateUserId.Equals(d.CreateUserId)).Select(c => c.BusinessTripDays).FirstOrDefault(),
                FromTheme = CompletionReports.Where(c => c.ServiceOrderId == d.ServiceOrderId && c.CreateUserId.Equals(d.CreateUserId)).Select(c => c.FromTheme).FirstOrDefault(),
                SalesMan = ServiceOrders.Where(s => s.Id == d.ServiceOrderId).Select(s => s.SalesMan).FirstOrDefault()
            });

            result.Count = ReimburseInfos.Count();
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
            if (loginUser.Account == "App")
            {
                loginUser = await GetUserId(Convert.ToInt32(request.AppId));
            }

            var orgids = await UnitWork.Find<Relevance>(r => r.Key == "UserOrg" && r.FirstId == loginUser.Id).Select(r => r.SecondId).ToListAsync();
            var orgname = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(o => orgids.Contains(o.Id)).OrderBy(o => o.CascadeId).Select(o => o.Name).FirstOrDefaultAsync();
            var CompletionReports = await UnitWork.Find<CompletionReport>(c => c.CreateUserId.Equals(loginUser.Id) && c.IsReimburse < 2).OrderByDescending(c => c.CreateTime).ToListAsync();
            //List<string> CompletionReportIds = new List<string>();
            //if (CompletionReports.Count > 0) CompletionReportIds = CompletionReports.Select(c=>c.Id).ToList();
            //CompletionReportIds = await UnitWork.Find<CompletionReport>(c=>!CompletionReportIds.Contains(c.Id)).Select(c => c.Id).ToListAsync();
            //var CompletionReportCount = await UnitWork.Find<ServiceWorkOrder>(s => CompletionReportIds.Contains(s.CompletionReportId) && s.CurrentUserNsapId.Equals(loginUser.Id)).Select(s => new { s.CurrentUserNsapId, s.CompletionReportId }).Distinct().ToListAsync();
            //CompletionReportIds = CompletionReportCount.Select(c => c.CompletionReportId).ToList();
            //CompletionReports.AddRange(await UnitWork.Find<CompletionReport>(c => CompletionReportIds.Contains(c.Id) && c.IsReimburse < 2).OrderByDescending(c => c.CreateTime).ToListAsync());


            var ids = CompletionReports.Select(c => c.ServiceOrderId).Distinct().ToList();
            var ServiceOrders = await UnitWork.Find<ServiceOrder>(s => ids.Contains(s.Id)).Include(s => s.ServiceWorkOrders).ToListAsync();
            List<ServiceOrder> ServiceOrderList = new List<ServiceOrder>();
            foreach (var item in ServiceOrders)
            {
                var WorkOrders = item.ServiceWorkOrders.Where(s => s.CurrentUserNsapId == loginUser.Id && s.Status <= 6).ToList();
                if (WorkOrders.Count == 0)
                {
                    ServiceOrderList.Add(item);
                }
            }
            var objs = ServiceOrderList.Select(s => new
            {
                UserId = loginUser.Id,
                UserName = loginUser.Name,
                ServiceRelations = loginUser.ServiceRelations == null ? "未录入" : loginUser.ServiceRelations,
                s.TerminalCustomer,
                s.TerminalCustomerId,
                s.Id,
                s.U_SAP_ID,
                OrgName = orgname,
                FromTheme = CompletionReports.Where(c => c.ServiceOrderId == s.Id).Select(c => c.FromTheme).FirstOrDefault(),
                Becity = CompletionReports.Where(c => c.ServiceOrderId == s.Id).Select(c => c.Becity).FirstOrDefault(),
                Destination = CompletionReports.Where(c => c.ServiceOrderId == s.Id).Select(c => c.Destination).FirstOrDefault(),
                BusinessTripDate = CompletionReports.Where(c => c.ServiceOrderId == s.Id).Select(c => c.BusinessTripDate).FirstOrDefault(),
                EndDate = CompletionReports.Where(c => c.ServiceOrderId == s.Id).Select(c => c.EndDate).FirstOrDefault(),
                MaterialCode = CompletionReports.Where(c => c.ServiceOrderId == s.Id).Select(c => c.MaterialCode).FirstOrDefault() == "其他设备" ? "其他设备" : CompletionReports.Where(c => c.ServiceOrderId == s.Id).Select(c => c.MaterialCode).FirstOrDefault().Substring(0, CompletionReports.Where(c => c.ServiceOrderId == s.Id).Select(c => c.MaterialCode).FirstOrDefault().IndexOf("-"))
            }).ToList();

            if (!string.IsNullOrWhiteSpace(request.SapId))
            {
                objs = objs.Where(s => s.U_SAP_ID.ToString().Contains(request.SapId) || s.TerminalCustomer.Contains(request.SapId)).ToList();
            }
            var result = new TableData();
            result.Data = objs.OrderBy(u => u.U_SAP_ID)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToList();
            result.Count = objs.Count;
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
                        .Include(r => r.ReimurseOperationHistories)
                        .FirstOrDefaultAsync();
            #region 获取附件
            Reimburse.ReimburseAttachments = await UnitWork.Find<ReimburseAttachment>(r => r.ReimburseId == ReimburseInfoId && r.ReimburseType == 0).ToListAsync();
            var ReimburseResp = Reimburse.MapTo<ReimburseInfoResp>();
            List<string> fileids = Reimburse.ReimburseAttachments.Select(r => r.FileId).ToList();
            List<ReimburseAttachment> rffilemodel = new List<ReimburseAttachment>();
            if (ReimburseResp.ReimburseFares != null && ReimburseResp.ReimburseFares.Count > 0) 
            {
                var rfids = ReimburseResp.ReimburseFares.Select(r => r.Id).ToList();
                rffilemodel = await UnitWork.Find<ReimburseAttachment>(r => rfids.Contains(r.ReimburseId) && r.ReimburseType == 2).ToListAsync();
            }
            if (ReimburseResp.ReimburseAccommodationSubsidies != null && ReimburseResp.ReimburseAccommodationSubsidies.Count > 0)
            {
                var rasids = ReimburseResp.ReimburseAccommodationSubsidies.Select(r => r.Id).ToList();
                rffilemodel.AddRange(await UnitWork.Find<ReimburseAttachment>(r => rasids.Contains(r.ReimburseId) && r.ReimburseType == 3).ToListAsync());
            }
            if (ReimburseResp.ReimburseOtherCharges != null && ReimburseResp.ReimburseOtherCharges.Count > 0) 
            {
                var rocids = ReimburseResp.ReimburseOtherCharges.Select(r => r.Id).ToList();
                rffilemodel.AddRange(await UnitWork.Find<ReimburseAttachment>(r => rocids.Contains(r.ReimburseId) && r.ReimburseType == 4).ToListAsync());
            }
            fileids.AddRange(rffilemodel.Select(f => f.FileId).ToList());

            var file = await UnitWork.Find<UploadFile>(f=> fileids.Contains(f.Id)).ToListAsync();
           
            ReimburseResp.ReimburseAttachments.ForEach(r => { r.AttachmentName = file.Where(f => f.Id.Equals(r.FileId)).Select(f => f.FileName).FirstOrDefault();r.FileType = file.Where(f => f.Id.Equals(r.FileId)).Select(f => f.FileType).FirstOrDefault(); });
            //List<ReimburseAttachment> filemodel = new List<ReimburseAttachment>();
            if (ReimburseResp.ReimburseFares != null && ReimburseResp.ReimburseFares.Count > 0) 
            {
                ReimburseResp.ReimburseFares.ForEach(r => r.ReimburseAttachments = rffilemodel.Where(f => f.ReimburseId.Equals(r.Id) && f.ReimburseType==2).Select(r => new ReimburseAttachmentResp
                {
                    Id = r.Id,
                    FileId = r.FileId,
                    AttachmentName = file.Where(f => f.Id.Equals(r.FileId)).Select(f => f.FileName).FirstOrDefault(),
                    FileType= file.Where(f => f.Id.Equals(r.FileId)).Select(f => f.FileType).FirstOrDefault(),
                    ReimburseId = r.ReimburseId,
                    ReimburseType = r.ReimburseType,
                    AttachmentType = r.AttachmentType
                }).ToList());
            }
            foreach (var item in ReimburseResp.ReimburseAccommodationSubsidies)
            {
                ReimburseResp.ReimburseAccommodationSubsidies.ForEach(r=>r.ReimburseAttachments = rffilemodel.Where(f => f.ReimburseId.Equals(r.Id) && f.ReimburseType == 3).Select(r => new ReimburseAttachmentResp
                {
                    Id = r.Id,
                    FileId = r.FileId,
                    AttachmentName = file.Where(f => f.Id.Equals(r.FileId)).Select(f => f.FileName).FirstOrDefault(),
                    FileType = file.Where(f => f.Id.Equals(r.FileId)).Select(f => f.FileType).FirstOrDefault(),
                    ReimburseId = r.ReimburseId,
                    ReimburseType = r.ReimburseType,
                    AttachmentType = r.AttachmentType
                }).ToList());
            }
            foreach (var item in ReimburseResp.ReimburseOtherCharges)
            {
                ReimburseResp.ReimburseOtherCharges.ForEach(r=>r.ReimburseAttachments = rffilemodel.Where(f => f.ReimburseId.Equals(r.Id) && f.ReimburseType == 4).Select(r => new ReimburseAttachmentResp
                {
                    Id = r.Id,
                    FileId = r.FileId,
                    AttachmentName = file.Where(f => f.Id.Equals(r.FileId)).Select(f => f.FileName).FirstOrDefault(),
                    FileType = file.Where(f => f.Id.Equals(r.FileId)).Select(f => f.FileType).FirstOrDefault(),
                    ReimburseId = r.ReimburseId,
                    ReimburseType = r.ReimburseType,
                    AttachmentType = r.AttachmentType
                }).ToList());
            }
            ReimburseResp.ReimurseOperationHistories = ReimburseResp.ReimurseOperationHistories.OrderBy(r => r.CreateTime).ToList();
            #endregion

            var orgids = await UnitWork.Find<Relevance>(r => r.Key == "UserOrg" && r.FirstId == ReimburseResp.CreateUserId).Select(r => r.SecondId).ToListAsync();
            var orgname = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(o => orgids.Contains(o.Id)).OrderBy(o => o.CascadeId).Select(o => o.Name).FirstOrDefaultAsync();
            var ServiceOrders = await UnitWork.Find<ServiceOrder>(s => s.Id == ReimburseResp.ServiceOrderId).FirstOrDefaultAsync();
            var CompletionReports = await UnitWork.Find<CompletionReport>(c => c.ServiceOrderId == ReimburseResp.ServiceOrderId && c.CreateUserId.Equals(ReimburseResp.CreateUserId)).FirstOrDefaultAsync();

            result.Data = new
            {
                ReimburseResp = ReimburseResp,
                UserName = await UnitWork.Find<User>(u => u.Id.Equals(ReimburseResp.CreateUserId)).Select(u => u.Name).FirstOrDefaultAsync(),
                OrgName = orgname,
                TerminalCustomer = ServiceOrders.TerminalCustomer,
                TerminalCustomerId = ServiceOrders.TerminalCustomerId,
                FromTheme = CompletionReports.FromTheme,
                Becity = CompletionReports.Becity,
                Destination = CompletionReports.Destination,
                BusinessTripDate = CompletionReports.BusinessTripDate,
                EndDate = CompletionReports.EndDate,
                MaterialCode = CompletionReports.MaterialCode == "其他设备" ? "其他设备" : CompletionReports.MaterialCode.Substring(0, CompletionReports.MaterialCode.IndexOf("-"))

            };

            return result;
        }

        /// <summary>
        /// 保存报销单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Add(AddOrUpdateReimburseInfoReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            if (loginUser.Account == "App")
            {
                loginUser = await GetUserId(Convert.ToInt32(req.AppId));
            }
            #region 报销单唯一
            var ReimburseCount=await UnitWork.Find<ReimburseInfo>(r => r.ServiceOrderId.Equals(req.ServiceOrderId) && r.CreateUserId.Equals(loginUser.Id)).CountAsync();
            if (ReimburseCount > 0) 
            {
                throw new CommonException("该服务单已提交报销单，不可二次使用！", Define.INVALID_ReimburseAgain);
            }
            #endregion

            #region 删除我的费用

            req.ReimburseFares = req.ReimburseFares.Where(r => r.IsAdd == null || r.IsAdd == true).ToList();
            req.ReimburseAccommodationSubsidies = req.ReimburseAccommodationSubsidies.Where(r => r.IsAdd == null || r.IsAdd == true).ToList();
            req.ReimburseOtherCharges = req.ReimburseOtherCharges.Where(r => r.IsAdd == null || r.IsAdd == true).ToList();
            if (req.MyexpendsIds != null && req.MyexpendsIds.Count > 0)
            {
                
                var myexpends = await UnitWork.Find<MyExpends>(m => req.MyexpendsIds.Contains(m.Id)).ToListAsync();
                myexpends.ForEach(m => m.IsDelete = true);
                await UnitWork.BatchUpdateAsync(myexpends.ToArray());
            }

            #endregion

            #region 判断发票是否唯一
            List<string> InvoiceNumbers = new List<string>();
            InvoiceNumbers.AddRange(req.ReimburseFares.Select(r => r.InvoiceNumber).ToList());
            InvoiceNumbers.AddRange(req.ReimburseAccommodationSubsidies.Select(r => r.InvoiceNumber).ToList());
            InvoiceNumbers.AddRange(req.ReimburseOtherCharges.Select(r => r.InvoiceNumber).ToList());
            bool IsInvoiceNumber = InvoiceNumbers.GroupBy(i => i).Where(g => g.Count() > 1).Count() >= 1;
            if (IsInvoiceNumber)
            {
                throw new CommonException("添加报销单失败。发票存在已使用，不可二次使用！", Define.INVALID_InvoiceNumber);
            }
            else if (InvoiceNumbers.Count() > 0)
            {
                if (!await IsSole(InvoiceNumbers))
                {
                    throw new CommonException("添加报销单失败。发票存在已使用，不可二次使用！", Define.INVALID_InvoiceNumber);
                }
            }
            #endregion

            var obj = req.MapTo<ReimburseInfo>();
            obj.ReimburseTravellingAllowances.ForEach(r => r.CreateTime = DateTime.Now);
            obj.ReimburseOtherCharges.ForEach(r => r.CreateTime = DateTime.Now);
            obj.ReimburseFares.ForEach(r => r.CreateTime = DateTime.Now);
            obj.ReimburseAccommodationSubsidies.ForEach(r => r.CreateTime = DateTime.Now);

            //用信号量代替锁
            await semaphoreSlim.WaitAsync();
            try
            {
                if (!obj.IsDraft)
                {
                    var maxmainid = await UnitWork.Find<ReimburseInfo>(null).OrderByDescending(r => r.MainId).Select(r => r.MainId).FirstOrDefaultAsync();
                    obj.MainId = maxmainid + 1;
                }
                obj.CreateTime = DateTime.Now;
                obj.RemburseStatus = 3;
                obj.CreateUserId = loginUser.Id;
                obj.IsRead = 1;
                obj = await UnitWork.AddAsync<ReimburseInfo, int>(obj);
                await UnitWork.SaveAsync();
            }
            finally
            {
                semaphoreSlim.Release();
            }

            var orgids = await UnitWork.Find<Relevance>(r => r.Key == "UserOrg" && r.FirstId == loginUser.Id).Select(r => r.SecondId).ToListAsync();
            var orgid = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(o => orgids.Contains(o.Id)).OrderBy(o => o.CascadeId).Select(o => o.Id).FirstOrDefaultAsync();

            if (!obj.IsDraft)
            {
                obj.RemburseStatus = 4;
                //创建报销流程
                var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name.Equals("报销"));
                var afir = new AddFlowInstanceReq();
                afir.SchemeId = mf.FlowSchemeId;
                afir.FrmType = 2;
                afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                afir.CustomName = $"报销" + DateTime.Now;
                afir.FrmData = $"{{\"ReimburseInfoId\":\"{obj.Id}\"}}";
                afir.OrgId = orgid;
                var FlowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(afir);
                obj.FlowInstanceId = FlowInstanceId;
                await UnitWork.UpdateAsync<ReimburseInfo>(obj);
                await UnitWork.AddAsync<ReimurseOperationHistory>(new ReimurseOperationHistory
                {
                    Action = "提交报销单",
                    CreateUser = loginUser.Name,
                    CreateUserId = loginUser.Id,
                    CreateTime = DateTime.Now,
                    ReimburseInfoId = obj.Id
                });
            }
            //反写完工报告
            var CompletionReports = await UnitWork.Find<CompletionReport>(c => c.ServiceOrderId == obj.ServiceOrderId && c.CreateUserId == obj.CreateUserId).ToListAsync();
            CompletionReports.ForEach(c => c.IsReimburse = 2);
            await UnitWork.BatchUpdateAsync<CompletionReport>(CompletionReports.ToArray());

            await UnitWork.SaveAsync();
            #region 保存附件
            List<ReimburseAttachment> filemodel = new List<ReimburseAttachment>();

            if (req.ReimburseAttachments != null && req.ReimburseAttachments.Count > 0)
            {
                filemodel = req.ReimburseAttachments.Where(r => r.IsAdd == true).MapToList<ReimburseAttachment>();
                filemodel.ForEach(f => { f.ReimburseId = obj.Id; f.ReimburseType = 0; f.Id = Guid.NewGuid().ToString(); });
                if (filemodel.Count > 0) await UnitWork.BatchAddAsync<ReimburseAttachment>(filemodel.ToArray());
            }
            var rac = await UnitWork.Find<ReimburseFare>(r => r.ReimburseInfoId == obj.Id).ToListAsync();
            foreach (var item in rac)
            {
                var racreq = req.ReimburseFares.Where(r => r.SerialNumber == item.SerialNumber).Select(r => r.ReimburseAttachments).FirstOrDefault();
                if (racreq != null && racreq.Count > 0)
                {
                    filemodel = racreq.Where(r => r.IsAdd == true).MapToList<ReimburseAttachment>();
                    filemodel.ForEach(f => { f.ReimburseId = item.Id; f.ReimburseType = 2; f.Id = Guid.NewGuid().ToString(); });
                    if (filemodel.Count > 0) await UnitWork.BatchAddAsync<ReimburseAttachment>(filemodel.ToArray());
                }
            }

            var ras = await UnitWork.Find<ReimburseAccommodationSubsidy>(r => r.ReimburseInfoId == obj.Id).ToListAsync();
            foreach (var item in ras)
            {
                var rasreq = req.ReimburseAccommodationSubsidies.Where(r => r.SerialNumber == item.SerialNumber).Select(r => r.ReimburseAttachments).FirstOrDefault();
                if (rasreq != null && rasreq.Count > 0)
                {
                    filemodel = rasreq.Where(r => r.IsAdd == true).MapToList<ReimburseAttachment>();
                    filemodel.ForEach(f => { f.ReimburseId = item.Id; f.ReimburseType = 3; f.Id = Guid.NewGuid().ToString(); });
                    if (filemodel.Count > 0) await UnitWork.BatchAddAsync<ReimburseAttachment>(filemodel.ToArray());
                }
            }

            var roc = await UnitWork.Find<ReimburseOtherCharges>(r => r.ReimburseInfoId == obj.Id).ToListAsync();
            foreach (var item in roc)
            {
                var rocreq = req.ReimburseOtherCharges.Where(r => r.SerialNumber == item.SerialNumber).Select(r => r.ReimburseAttachments).FirstOrDefault();
                if (rocreq != null && rocreq.Count > 0)
                {
                    filemodel = rocreq.Where(r => r.IsAdd == true).MapToList<ReimburseAttachment>();
                    filemodel.ForEach(f => { f.ReimburseId = item.Id; f.ReimburseType = 4; f.Id = Guid.NewGuid().ToString(); });
                    if (filemodel.Count > 0) await UnitWork.BatchAddAsync<ReimburseAttachment>(filemodel.ToArray());
                }
            }
            await UnitWork.SaveAsync();
            #endregion

        }

        /// <summary>
        /// 修改报销单 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task UpDate(AddOrUpdateReimburseInfoReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == "App")
            {
                loginUser = await GetUserId(Convert.ToInt32(req.AppId));
            }

            #region 删除我的费用

            req.ReimburseFares = req.ReimburseFares.Where(r => r.IsAdd == null || r.IsAdd == true).ToList();
            req.ReimburseAccommodationSubsidies = req.ReimburseAccommodationSubsidies.Where(r => r.IsAdd == null || r.IsAdd == true).ToList();
            req.ReimburseOtherCharges = req.ReimburseOtherCharges.Where(r => r.IsAdd == null || r.IsAdd == true).ToList();
            if (req.MyexpendsIds != null && req.MyexpendsIds.Count > 0)
            {
                var myexpends = await UnitWork.Find<MyExpends>(m => req.MyexpendsIds.Contains(m.Id)).ToListAsync();
                myexpends.ForEach(m => m.IsDelete = true);
                await UnitWork.BatchUpdateAsync(myexpends.ToArray());
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
                throw new CommonException("添加报销单失败。发票存在已使用，不可二次使用！", Define.INVALID_InvoiceNumber);
            }
            else if (InvoiceNumbers.Count() > 0)
            {
                if (!await IsSole(InvoiceNumbers))
                {
                    throw new CommonException("添加报销单失败。发票存在已使用，不可二次使用！", Define.INVALID_InvoiceNumber);
                }
            }
            #endregion


            await semaphoreSlim.WaitAsync();
            var obj = req.MapTo<ReimburseInfo>();

            try
            {
                if (!obj.IsDraft && string.IsNullOrWhiteSpace(req.FlowInstanceId) && (string.IsNullOrWhiteSpace(obj.MainId.ToString()) || obj.MainId == 0))
                {
                    var maxmainid = await UnitWork.Find<ReimburseInfo>(null).OrderByDescending(r => r.MainId).Select(r => r.MainId).FirstOrDefaultAsync();
                    obj.MainId = maxmainid + 1;
                }
                obj.RemburseStatus = 3;
                obj.IsRead = 1;
                await UnitWork.UpdateAsync<ReimburseInfo>(r => r.Id == obj.Id, r => new ReimburseInfo
                {
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
            }
            finally
            {
                semaphoreSlim.Release();
            }

            if (!req.IsDraft && string.IsNullOrWhiteSpace(req.FlowInstanceId))
            {
                var orgids = await UnitWork.Find<Relevance>(r => r.Key == "UserOrg" && r.FirstId == loginUser.Id).Select(r => r.SecondId).ToListAsync();
                var orgid = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(o => orgids.Contains(o.Id)).OrderBy(o => o.CascadeId).Select(o => o.Id).FirstOrDefaultAsync();

                var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name.Equals("报销"));
                var afir = new AddFlowInstanceReq();
                afir.SchemeId = mf.FlowSchemeId;
                afir.FrmType = 2;
                afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                afir.CustomName = $"报销";
                afir.FrmData = "{ReimburseInfoId:" + obj.Id + "}";
                afir.OrgId = orgid;
                var FlowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(afir);
                await UnitWork.AddAsync<ReimurseOperationHistory>(new ReimurseOperationHistory
                {
                    Action = "提交报销单",
                    CreateUser = loginUser.Name,
                    CreateUserId = loginUser.Id,
                    CreateTime = DateTime.Now,
                    ReimburseInfoId = obj.Id
                });
                obj.RemburseStatus = 4;
                obj.IsRead = 1;
                obj.FlowInstanceId = FlowInstanceId;
                await UnitWork.UpdateAsync<ReimburseInfo>(r => r.Id == obj.Id, r => new ReimburseInfo
                {
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
            }
            

            #region 删除
            //附件删除
            if (req.FileId != null && req.FileId.Count != 0)
            {
                var delfiles = await UnitWork.Find<ReimburseAttachment>(r => req.FileId.Contains(r.Id)).ToListAsync();
                delfiles.ForEach(r => UnitWork.Delete<ReimburseAttachment>(r));
            }
            //删除出差补贴
            if (req.DelteReimburse != null && req.DelteReimburse.Count != 0)
            {
                var delids = req.DelteReimburse.Where(r => r.ReimburseType == 1).Select(r => r.DeleteId).ToList();
                if (delids.Count > 0)
                {
                    var delrta = await UnitWork.Find<ReimburseTravellingAllowance>(r => delids.Contains(r.Id) && r.ReimburseInfoId == req.Id).ToListAsync();
                    delrta.ForEach(r => UnitWork.Delete<ReimburseTravellingAllowance>(r));
                }
                //删除交通补贴及附件
                delids = req.DelteReimburse.Where(r => r.ReimburseType == 2).Select(r => r.DeleteId).ToList();
                if (delids.Count > 0)
                {
                    var ReimburseAttachmentIds = await UnitWork.Find<ReimburseAttachment>(r => delids.Contains(r.ReimburseId) && r.ReimburseType==2).ToListAsync();
                    ReimburseAttachmentIds.ForEach( r => UnitWork.Delete<ReimburseAttachment>(r));

                    var delrf = await UnitWork.Find<ReimburseFare>(r => delids.Contains(r.Id) && r.ReimburseInfoId == req.Id).ToListAsync();
                    delrf.ForEach(r => UnitWork.DeleteAsync<ReimburseFare>(r));
                }
                //删除住宿补贴及附件
                delids = req.DelteReimburse.Where(r => r.ReimburseType == 3).Select(r => r.DeleteId).ToList();
                if (delids.Count > 0)
                {
                    var ReimburseAttachmentIds = await UnitWork.Find<ReimburseAttachment>(r => delids.Contains(r.ReimburseId) && r.ReimburseType == 3).ToListAsync();
                    ReimburseAttachmentIds.ForEach(r => UnitWork.Delete<ReimburseAttachment>(r));

                    var delras = await UnitWork.Find<ReimburseAccommodationSubsidy>(r => delids.Contains(r.Id) && r.ReimburseInfoId == req.Id).ToListAsync();
                    delras.ForEach(r => UnitWork.Delete<ReimburseAccommodationSubsidy>(r));
                }
                //删除其他补贴及附件
                delids = req.DelteReimburse.Where(r => r.ReimburseType == 4).Select(r => r.DeleteId).ToList();
                if (delids.Count > 0)
                {
                    var ReimburseAttachmentIds = await UnitWork.Find<ReimburseAttachment>(r => delids.Contains(r.ReimburseId) && r.ReimburseType == 4).ToListAsync();
                    ReimburseAttachmentIds.ForEach(r => UnitWork.Delete<ReimburseAttachment>(r));

                    var delroc = await UnitWork.Find<ReimburseOtherCharges>(r => delids.Contains(r.Id) && r.ReimburseInfoId == req.Id).ToListAsync();
                    delroc.ForEach(r => UnitWork.Delete<ReimburseOtherCharges>(r));
                }

            }

            #endregion

            #region 修改
            //修改出差补贴

            if (req.ReimburseTravellingAllowances != null && req.ReimburseTravellingAllowances.Count > 0)
            {
                var Updaterta = req.ReimburseTravellingAllowances.Where(r => !string.IsNullOrWhiteSpace(r.Id.ToString()) && r.Id != 0).ToList().MapToList<ReimburseTravellingAllowance>();
                await UnitWork.BatchUpdateAsync<ReimburseTravellingAllowance>(Updaterta.ToArray());
            }
            //修改交通费用
            if (req.ReimburseFares != null && req.ReimburseFares.Count > 0)
            {
                var Updaterf = req.ReimburseFares.Where(r => !string.IsNullOrWhiteSpace(r.Id.ToString()) && r.Id != 0).ToList().MapToList<ReimburseFare>();
                await UnitWork.BatchUpdateAsync<ReimburseFare>(Updaterf.ToArray());
            }
            //修改住宿补贴
            if (req.ReimburseAccommodationSubsidies != null && req.ReimburseAccommodationSubsidies.Count > 0)
            {
                var Updateras = req.ReimburseAccommodationSubsidies.Where(r => !string.IsNullOrWhiteSpace(r.Id.ToString()) && r.Id != 0).ToList().MapToList<ReimburseAccommodationSubsidy>();
                await UnitWork.BatchUpdateAsync<ReimburseAccommodationSubsidy>(Updateras.ToArray());
            }
            //修改其他补贴
            if (req.ReimburseOtherCharges != null && req.ReimburseOtherCharges.Count > 0)
            {
                var Updateroc = req.ReimburseOtherCharges.Where(r => !string.IsNullOrWhiteSpace(r.Id.ToString()) && r.Id != 0).ToList().MapToList<ReimburseOtherCharges>();
                await UnitWork.BatchUpdateAsync<ReimburseOtherCharges>(Updateroc.ToArray());
            }
            #endregion

            #region 新增

            if (req.ReimburseTravellingAllowances != null && req.ReimburseTravellingAllowances.Count > 0)
            {
                var addrta = req.ReimburseTravellingAllowances.Where(r => string.IsNullOrWhiteSpace(r.Id.ToString()) || r.Id == 0).MapToList<ReimburseTravellingAllowance>();
                if (addrta.Count > 0)
                {
                    addrta.ForEach(a => a.ReimburseInfoId = obj.Id);
                    await UnitWork.BatchAddAsync<ReimburseTravellingAllowance, int>(addrta.ToArray());
                }
            }
            if (req.ReimburseFares != null && req.ReimburseFares.Count > 0)
            {
                var addrf = req.ReimburseFares.Where(r => string.IsNullOrWhiteSpace(r.Id.ToString()) || r.Id == 0).MapToList<ReimburseFare>();
                if (addrf.Count > 0)
                {
                    addrf.ForEach(a => a.ReimburseInfoId = obj.Id);
                    await UnitWork.BatchAddAsync<ReimburseFare, int>(addrf.ToArray());
                }
            }
            if (req.ReimburseAccommodationSubsidies != null && req.ReimburseAccommodationSubsidies.Count > 0)
            {
                var addras = req.ReimburseAccommodationSubsidies.Where(r => string.IsNullOrWhiteSpace(r.Id.ToString()) || r.Id == 0).MapToList<ReimburseAccommodationSubsidy>();
                if (addras.Count > 0)
                {
                    addras.ForEach(a => a.ReimburseInfoId = obj.Id);
                    await UnitWork.BatchAddAsync<ReimburseAccommodationSubsidy, int>(addras.ToArray());
                }
            }
            if (req.ReimburseOtherCharges != null && req.ReimburseOtherCharges.Count > 0)
            {
                var addroc = req.ReimburseOtherCharges.Where(r => string.IsNullOrWhiteSpace(r.Id.ToString()) || r.Id == 0).MapToList<ReimburseOtherCharges>();
                if (addroc.Count > 0)
                {
                    addroc.ForEach(a => a.ReimburseInfoId = obj.Id);
                    await UnitWork.BatchAddAsync<ReimburseOtherCharges, int>(addroc.ToArray());
                }
            }
            await UnitWork.SaveAsync();
            #endregion

            

            #region 保存新附件
            List<ReimburseAttachment> filemodel = new List<ReimburseAttachment>();
            if (req.ReimburseAttachments != null && req.ReimburseAttachments.Count > 0)
            {
                filemodel = req.ReimburseAttachments.Where(r => (string.IsNullOrWhiteSpace(r.Id) || r.Id == "0") && r.IsAdd == true).MapToList<ReimburseAttachment>();
                filemodel.ForEach(f => { f.ReimburseId = obj.Id; f.ReimburseType = 0; f.Id = Guid.NewGuid().ToString(); });
                if (filemodel.Count > 0) await UnitWork.BatchAddAsync<ReimburseAttachment>(filemodel.ToArray());
            }

            var rac = await UnitWork.Find<ReimburseFare>(r => r.ReimburseInfoId == obj.Id).ToListAsync();
            foreach (var item in rac)
            {
                var racreq = req.ReimburseFares.Where(r => r.SerialNumber == item.SerialNumber).Select(r => r.ReimburseAttachments).FirstOrDefault();
                if (racreq != null && racreq.Count > 0)
                {
                    racreq = racreq.Where(r => (string.IsNullOrWhiteSpace(r.Id) || r.Id == "0") && r.IsAdd == true).ToList();
                    filemodel = racreq.MapToList<ReimburseAttachment>();
                    filemodel.ForEach(f => { f.ReimburseId = item.Id; f.ReimburseType = 2; f.Id = Guid.NewGuid().ToString(); });
                    if (filemodel.Count > 0) await UnitWork.BatchAddAsync<ReimburseAttachment>(filemodel.ToArray());
                }
            }
            var ras = await UnitWork.Find<ReimburseAccommodationSubsidy>(r => r.ReimburseInfoId == obj.Id).ToListAsync();

            foreach (var item in ras)
            {
                var rasreq = req.ReimburseAccommodationSubsidies.Where(r => r.SerialNumber == item.SerialNumber).Select(r => r.ReimburseAttachments).FirstOrDefault();
                if (rasreq != null && rasreq.Count > 0)
                {
                    rasreq = rasreq.Where(r => (string.IsNullOrWhiteSpace(r.Id) || r.Id == "0") && r.IsAdd == true).ToList();
                    filemodel = rasreq.MapToList<ReimburseAttachment>();
                    filemodel.ForEach(f => { f.ReimburseId = item.Id; f.ReimburseType = 3; f.Id = Guid.NewGuid().ToString(); });
                    if (filemodel.Count > 0) await UnitWork.BatchAddAsync<ReimburseAttachment>(filemodel.ToArray());
                }
            }

            var roc = await UnitWork.Find<ReimburseOtherCharges>(r => r.ReimburseInfoId == obj.Id).ToListAsync();

            foreach (var item in roc)
            {
                var rocreq = req.ReimburseOtherCharges.Where(r => r.SerialNumber == item.SerialNumber).Select(r => r.ReimburseAttachments).FirstOrDefault();
                if (rocreq != null && rocreq.Count > 0)
                {
                    rocreq = rocreq.Where(r => (string.IsNullOrWhiteSpace(r.Id) || r.Id == "0") && r.IsAdd == true).ToList();
                    filemodel = rocreq.MapToList<ReimburseAttachment>();
                    filemodel.ForEach(f => { f.ReimburseId = item.Id; f.ReimburseType = 4; f.Id = Guid.NewGuid().ToString(); });
                    if (filemodel.Count > 0) await UnitWork.BatchAddAsync<ReimburseAttachment>(filemodel.ToArray());
                }
            }
            await UnitWork.SaveAsync();
            #endregion
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

            var obj = await UnitWork.Find<ReimburseInfo>(r => r.Id == req.Id).FirstOrDefaultAsync();
            obj.ShortCustomerName = req.ShortCustomerName;
            obj.ProjectName = req.ProjectName;
            obj.BearToPay = req.BearToPay;
            obj.ReimburseType = req.ReimburseType;
            obj.Responsibility = req.Responsibility;
            eoh.ApprovalStage = obj.RemburseStatus;
            if (req.IsReject)
            {
                List<string> ids = new List<string>();
                ids.Add(obj.FlowInstanceId);
                await _flowInstanceApp.DeleteAsync(ids.ToArray());
                //_flowInstanceApp.Verification(new VerificationReq
                //{
                //    NodeRejectStep = "",
                //    NodeRejectType = "0",
                //    FlowInstanceId = req.FlowInstanceId,
                //    VerificationFinally = "3",
                //    VerificationOpinion = "驳回",
                //});
                obj.RemburseStatus = 2;
                obj.FlowInstanceId = "";
                eoh.ApprovalResult = "驳回";
            }
            else
            {
                eoh.ApprovalResult = "同意";
                if (loginContext.Roles.Any(r => r.Name.Equals("客服主管")))
                {
                    obj.RemburseStatus = 5;
                }
                else if (loginContext.Roles.Any(r => r.Name.Equals("财务初审")))
                {
                    obj.RemburseStatus = 6;
                }
                else if (loginContext.Roles.Any(r => r.Name.Equals("财务复审")))
                {
                    obj.RemburseStatus = 7;
                }
                else if (loginContext.Roles.Any(r => r.Name.Equals("总经理")))
                {
                    obj.RemburseStatus = 8;
                }
                else if (loginContext.Roles.Any(r => r.Name.Equals("出纳")))
                {
                    eoh.ApprovalResult = "已支付";
                    obj.RemburseStatus = 9;
                    obj.PayTime = DateTime.Now;
                }
                
                _flowInstanceApp.Verification(new VerificationReq
                {
                    NodeRejectStep = "",
                    NodeRejectType = "0",
                    FlowInstanceId = req.FlowInstanceId,
                    VerificationFinally = "1",
                    VerificationOpinion = "同意",
                });
            }

            await UnitWork.UpdateAsync<ReimburseInfo>(obj);
            var seleoh = await UnitWork.Find<ReimurseOperationHistory>(null).OrderByDescending(r => r.CreateTime).FirstOrDefaultAsync();
            eoh.Action = "审批";
            eoh.CreateUser = loginContext.User.Name;
            eoh.CreateUserId = loginContext.User.Id;
            eoh.CreateTime = DateTime.Now;
            eoh.ReimburseInfoId = obj.Id;
            eoh.Remark = req.Remark;
            eoh.IntervalTime = DateTime.Now.Subtract(Convert.ToDateTime(seleoh.CreateTime)).Minutes;
            await UnitWork.AddAsync<ReimurseOperationHistory>(eoh);
            await UnitWork.SaveAsync();

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
            if (loginUser.Account == "App")
            {
                loginUser = await GetUserId(Convert.ToInt32(req.AppId));
            }
            var result = new TableData();
            var obj = await UnitWork.Find<ReimburseInfo>(r => r.Id.Equals(req.ReimburseInfoId) && r.CreateUserId.Equals(loginUser.Id)).FirstOrDefaultAsync();
            if (obj != null && obj.IsRead == 1)
            {
                List<string> ids = new List<string>();
                ids.Add(obj.FlowInstanceId);
                await _flowInstanceApp.DeleteAsync(ids.ToArray());
                obj.RemburseStatus = 1;
                obj.IsDraft = true;
                obj.FlowInstanceId = null;
                obj.IsRead = 1;
                await UnitWork.UpdateAsync<ReimburseInfo>(obj);
                await UnitWork.AddAsync<ReimurseOperationHistory>(new ReimurseOperationHistory
                {
                    Action = "撤销报销单",
                    CreateUser = loginUser.Name,
                    CreateUserId = loginUser.Id,
                    CreateTime = DateTime.Now,
                    ReimburseInfoId = obj.Id
                });
                await UnitWork.SaveAsync();
                result.Code = 200;
                result.Message = "已撤销到草稿箱";
            }
            else
            {
                result.Code = 500;
                result.Message = "客服主管已读不可撤销！！！";
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
        /// <param name="ReimburseInfoId"></param>
        /// <returns></returns>
        public async Task Delete(int ReimburseInfoId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var Reimburse = await UnitWork.Find<ReimburseInfo>(r => r.Id == ReimburseInfoId)
                        //.Include(r => r.ReimburseAttachments)
                        .Include(r => r.ReimburseTravellingAllowances)
                        .Include(r => r.ReimburseFares)
                        .Include(r => r.ReimburseAccommodationSubsidies)
                        .Include(r => r.ReimburseOtherCharges)
                        .Include(r => r.ReimurseOperationHistories)
                        .FirstOrDefaultAsync();
            if (Reimburse != null)
            {
                var files = await UnitWork.Find<ReimburseAttachment>(null).ToListAsync();
                var delfiles = files.Where(f => f.ReimburseId.Equals(Reimburse.Id) && f.ReimburseType == 0).ToList();
                delfiles.ForEach(d => UnitWork.Delete<ReimburseAttachment>(d));
                foreach (var item in Reimburse.ReimburseFares)
                {
                    delfiles = files.Where(f => f.ReimburseId.Equals(item.Id) && f.ReimburseType == 2).ToList();
                    delfiles.ForEach(d => UnitWork.Delete<ReimburseAttachment>(d));
                    await UnitWork.DeleteAsync<ReimburseFare>(item);
                }
                foreach (var item in Reimburse.ReimburseAccommodationSubsidies)
                {
                    delfiles = files.Where(f => f.ReimburseId.Equals(item.Id) && f.ReimburseType == 3).ToList();
                    delfiles.ForEach(d => UnitWork.Delete<ReimburseAttachment>(d));
                    await UnitWork.DeleteAsync<ReimburseAccommodationSubsidy>(item);
                }
                foreach (var item in Reimburse.ReimburseOtherCharges)
                {
                    delfiles = files.Where(f => f.ReimburseId.Equals(item.Id) && f.ReimburseType == 4).ToList();
                    delfiles.ForEach(d => UnitWork.Delete<ReimburseAttachment>(d));
                    await UnitWork.DeleteAsync<ReimburseOtherCharges>(item);
                }
                Reimburse.ReimburseTravellingAllowances.ForEach(r => UnitWork.Delete<ReimburseTravellingAllowance>(r));
                Reimburse.ReimurseOperationHistories.ForEach(r => UnitWork.Delete<ReimurseOperationHistory>(r));
                await UnitWork.DeleteAsync<ReimburseInfo>(Reimburse);
                await UnitWork.SaveAsync();
            }

            var CompletionReports = await UnitWork.Find<CompletionReport>(c => c.ServiceOrderId == Reimburse.ServiceOrderId && c.CreateUserId == Reimburse.CreateUserId).ToListAsync();
            CompletionReports.ForEach(c => c.IsReimburse = 2);
            await UnitWork.BatchUpdateAsync<CompletionReport>(CompletionReports.ToArray());
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 打印报销单 
        /// </summary>
        /// <param name="ReimburseInfoId"></param>
        /// <returns></returns>
        public async Task Print(int ReimburseInfoId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var Reimburse = await UnitWork.Find<ReimburseInfo>(r => r.Id == ReimburseInfoId)
                        //.Include(r => r.ReimburseAttachments)
                        .Include(r => r.ReimburseTravellingAllowances)
                        .Include(r => r.ReimburseFares)
                        .Include(r => r.ReimburseAccommodationSubsidies)
                        .Include(r => r.ReimburseOtherCharges)
                        .Include(r => r.ReimurseOperationHistories)
                        .FirstOrDefaultAsync();

            var user = await UnitWork.Find<User>(u => u.Id.Equals(Reimburse.CreateUserId)).FirstOrDefaultAsync();
            var orgids = await UnitWork.Find<Relevance>(r => r.Key == "UserOrg" && r.FirstId == Reimburse.CreateUserId).Select(r => r.SecondId).ToListAsync();
            var orgname = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(o => orgids.Contains(o.Id)).OrderBy(o => o.CascadeId).Select(o => o.Name).FirstOrDefaultAsync();
            var CompletionReports = await UnitWork.Find<CompletionReport>(c => c.ServiceOrderId == Reimburse.ServiceOrderId && c.CreateUserId.Equals(Reimburse.CreateUserId)).OrderByDescending(c => c.CreateTime).FirstOrDefaultAsync();
            decimal? money = 0;
            if (Reimburse.ReimburseTravellingAllowances != null && Reimburse.ReimburseTravellingAllowances.Count > 0)
            {
                var rta = Reimburse.ReimburseTravellingAllowances.FirstOrDefault();
                money = rta.Money * rta.Days;
            }
            var Subsidy = money;
            if (Reimburse.ReimburseAccommodationSubsidies != null && Reimburse.ReimburseAccommodationSubsidies.Count > 0)
            {
                money = 0;
                Reimburse.ReimburseAccommodationSubsidies.ForEach(r => money += r.TotalMoney);
            }
            var PutUp = money;
            if (Reimburse.ReimburseOtherCharges != null && Reimburse.ReimburseOtherCharges.Count > 0)
            {
                money = 0;
                Reimburse.ReimburseOtherCharges.ForEach(r => money += r.Money);
            }
            var Else = money;
            decimal? Aircraft = 0, Train = 0, Coach = 0, Transport = 0;
            if (Reimburse.ReimburseFares != null && Reimburse.ReimburseFares.Count > 0)
            {
                Reimburse.ReimburseFares.Where(r => r.Transport == "1").ForEach(r => Aircraft += r.Money);
                Reimburse.ReimburseFares.Where(r => r.Transport == "2").ForEach(r => Train += r.Money);
                Reimburse.ReimburseFares.Where(r => r.Transport == "3").ForEach(r => Coach += r.Money);
                Reimburse.ReimburseFares.Where(r => r.Transport == "4").ForEach(r => Transport += r.Money);
            }

            var PrintReimburse = new PrintReimburseResp
            {
                StartTime = CompletionReports.BusinessTripDate,
                EndTime = CompletionReports.EndDate,
                Day = CompletionReports.BusinessTripDays,
                ReimburseId = Reimburse.MainId,
                OrgName = orgname,
                UserName = user.Name,
                Position = "",
                TerminalCustomer = CompletionReports.TerminalCustomer,
                FromTheme = CompletionReports.FromTheme,
                Subsidy = Subsidy,
                Else = Else,
                PutUp = PutUp,
                Aircraft = Aircraft,
                Train = Train,
                Coach = Coach,
                Transport = Transport,
                Total = TransformCharOrNumber.SumConvert(null, Convert.ToDecimal(Reimburse.TotalMoney)),
            };
            await ExportAllHandler.Exporterpdf(@"D:\工作\程序\前端\aaa.pdf", PrintReimburse, "PrintReimburse.cshtml");
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

        public ReimburseInfoApp(IUnitWork unitWork,
            RevelanceManagerApp app, ModuleFlowSchemeApp moduleFlowSchemeApp, FlowInstanceApp flowInstanceApp, IAuth auth) : base(unitWork, auth)
        {
            _revelanceApp = app;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
            _flowInstanceApp = flowInstanceApp;
        }
    }
}