using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenAuth.App.Interface;
using OpenAuth.App.Response;
using OpenAuth.App.SignalR;
using OpenAuth.Repository;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenAuth.App.Order;
using OpenAuth.App.Order.Request;

namespace OpenAuth.App.Serve
{
    public class ServiceOrderPushNotification: OnlyUnitWorkBaeApp
    {
        private readonly IHubContext<MessageHub> _hubContext;
        private IOptions<AppSetting> _appConfiguration;
        private ILogger<ServiceOrderPushNotification> _logger;
        private OrderWorkbenchApp _orderWorkbenchApp;

        public ServiceOrderPushNotification(OrderWorkbenchApp orderWorkbenchApp, ILogger<ServiceOrderPushNotification> logger, IUnitWork unitWork, IHubContext<MessageHub> hubContext, IAuth auth, IOptions<AppSetting> appConfiguration) : base(unitWork, auth)
        {
            _hubContext = hubContext;
            _appConfiguration=appConfiguration;
            _logger = logger;
            _orderWorkbenchApp = orderWorkbenchApp;
        }

        #region 定时推送
        /// <summary>
        /// 推送待处理数量
        /// </summary>
        /// <returns></returns>
        public async Task SendPendingNumber()
        {
            #region 服务单
            var ServiceOrderCount = await UnitWork.Find<ServiceOrder>(u => u.Status == 1).CountAsync();
            await _hubContext.Clients.Groups("呼叫中心").SendAsync("ServiceOrderCount", "系统", ServiceOrderCount);

            var result = new TableData();
            var model = UnitWork.Find<ServiceWorkOrder>(s => s.Status == 1).Select(s => s.ServiceOrderId).Distinct();
            var ids = await model.ToListAsync();
            var query = await UnitWork.Find<ServiceOrder>(s => ids.Contains(s.Id)).ToListAsync();
            var groub = query.GroupBy(s => s.Supervisor).ToList();

            foreach (var item in groub)
            {
                await _hubContext.Clients.User(item.Key).SendAsync("ServiceWordOrderCount", "系统", item.Count());
            }
            var approve = await UnitWork.Find<ServiceOrder>(c => c.AllowOrNot == 1 && c.Status == 2 && c.VestInOrg == 1).Select(c => c.SalesMan).ToListAsync();
            var count = approve.GroupBy(c => c).ToList();
            foreach (var item in count)
            {
                await _hubContext.Clients.User(item.Key).SendAsync("ServiceCount", "系统", item.Count());
            }

            var serviceOrderEcnCount = await UnitWork.Find<ServiceOrder>(u => u.Status == 2 && u.VestInOrg == 1 && u.FromId == 8 && u.ServiceWorkOrders.Any(s => s.Status < 7)).CountAsync();
            await _hubContext.Clients.Groups("呼叫中心").SendAsync("ServiceOrderECNCount", "系统", serviceOrderEcnCount);
            #endregion
            #region 报销单
            var reimburseInfos = await UnitWork.Find<ReimburseInfo>(r => r.RemburseStatus > 3 && r.RemburseStatus < 9).GroupBy(r => r.RemburseStatus).Select(r => new { status = r.Key, count = r.Count() }).ToListAsync();
            foreach (var item in reimburseInfos)
            {
                switch (item.status)
                {
                    case 4:
                        await _hubContext.Clients.Groups("客服主管").SendAsync("RemburseCount", "系统", item.count);
                        break;
                    case 5:
                        await _hubContext.Clients.Groups("财务初审").SendAsync("RemburseCount", "系统", item.count);
                        break;
                    case 6:
                        await _hubContext.Clients.Groups("财务复审").SendAsync("RemburseCount", "系统", item.count);
                        break;
                    case 7:
                        await _hubContext.Clients.Groups("总经理").SendAsync("RemburseCount", "系统", item.count);
                        break;
                    case 8:
                        await _hubContext.Clients.Groups("出纳").SendAsync("PaymentCount", "系统", item.count);
                        break;
                }
            }

            #endregion
            #region 报价单
            var quotations = await UnitWork.Find<Quotation>(q => q.QuotationStatus > 3 && q.QuotationStatus < 10).ToListAsync();
            var serviceOrderIds = quotations.Where(q => q.QuotationStatus == 3.1M).Select(q => q.ServiceOrderId).ToList();
            var quotationCounts = quotations.GroupBy(q => q.QuotationStatus).Select(r => new { status = r.Key, count = r.Count() }).OrderBy(r => r.status).ToList();
            var serviceOrders = await UnitWork.Find<ServiceOrder>(s => serviceOrderIds.Contains(s.Id)).Select(s => new { s.SalesMan, s.Id }).ToListAsync();
            var sales = quotations.Where(q => q.QuotationStatus == 3.1M).Select(q => new
            {
                q.Id,
                SalesMan = serviceOrders.Where(s => s.Id == q.ServiceOrderId).FirstOrDefault()?.SalesMan
            });
            var salesList = sales.GroupBy(s => s.SalesMan).Select(s => new { SalesMan = s.Key, count = s.Count() }).ToList();
            var num = salesList.Where(s => s.SalesMan == "韦京生").FirstOrDefault()?.count == null ? 0 : salesList.Where(s => s.SalesMan == "韦京生").FirstOrDefault()?.count;
            salesList = salesList.Where(s => s.SalesMan != "韦京生").ToList();
            foreach (var item in quotationCounts)
            {
                switch (item.status)
                {
                    case 3.1M:
                        foreach (var soitem in salesList)
                        {
                            await _hubContext.Clients.User(soitem.SalesMan).SendAsync("QuotationCount", "系统", soitem.count);
                        }
                        break;
                    case 4:
                        await _hubContext.Clients.Groups("物料工程审批").SendAsync("QuotationCount", "系统", item.count);
                        break;
                    case 5:
                        await _hubContext.Clients.Groups("总经理").SendAsync("QuotationCount", "系统", (item.count + num));
                        break;
                    case 8:
                        await _hubContext.Clients.Groups("物料财务").SendAsync("SalesOrderCount", "系统", item.count);
                        break;
                    case 10:
                        await _hubContext.Clients.Groups("仓库").SendAsync("StockOutCount", "系统", item.count);
                        break;
                }
            }
            #endregion
            #region 退料单
            //查询符合条件的用户和退料信息
            //查询退料单实例中,状态为开始的流程id
            var flowInstanceIds = await (from s in UnitWork.Find<FlowScheme>(null)
                                         join i in UnitWork.Find<FlowInstance>(null)
                                         on s.Id equals i.SchemeId
                                         where s.SchemeName == "退料单审批" && i.ActivityName == "开始"
                                         select i.Id).Distinct().ToListAsync();

            //根据流程id查找退料信息(流程id为空表示未提交,流程id包含的表示被驳回,需要重新提交)
            var returnNotes = await UnitWork.Find<ReturnNote>(null).Where(r => (flowInstanceIds.Contains(r.FlowInstanceId) || string.IsNullOrWhiteSpace(r.FlowInstanceId))).Select(r => new { r.Id, r.ServiceOrderSapId, r.SalesOrderId, r.CreateUser }).ToListAsync();

            //按提交人进行分组
            var groupData = returnNotes.GroupBy(r => r.CreateUser);
            //向每个提交人发送提醒消息
            foreach (var item in groupData)
            {
                var user = item.Key;
                await _hubContext.Clients.User(user).SendAsync("ReturnNoteUnSubmitCount", "系统", item.Count());
            }
            #endregion
            #region 责任归属
         
            var BlameBelongFlowInstanceIds = await UnitWork.Find<BlameBelong>(c => c.Status >= 1 && c.Status < 6).Select(a => a.FlowInstanceId).ToListAsync();
            var BlameBelongUserId = await UnitWork.Find<FlowInstance>(a => BlameBelongFlowInstanceIds.Contains(a.Id)).Select(a => a.MakerList).ToListAsync();

            List<string> BlameBelonglistUser = new List<string>();
            foreach (var item in BlameBelongUserId)
            {
                BlameBelonglistUser.AddRange(item.Split(","));
            }
            var BlameBelonguserCount = BlameBelonglistUser.GroupBy(a => a).Select(a => new { Id = a.Key, Count = a.Count() });
            BlameBelonglistUser = BlameBelonglistUser.Distinct().ToList();
            var BlameBelongUsers = await UnitWork.Find<User>(a => BlameBelonglistUser.Contains(a.Id)).ToListAsync();
            foreach (var item in BlameBelonguserCount)
            {
                var name = BlameBelongUsers.Where(a => a.Id == item.Id).FirstOrDefault()?.Name;
                if (!string.IsNullOrEmpty(name)  )
                {
                    await _hubContext.Clients.User(name).SendAsync("BlameBelongCount", "系统", item.Count);
                }
            }
            #endregion
            #region 结算单

            var flowInstanceInfos = await UnitWork.Find<FlowInstance>(f => f.CustomName == "个人代理结算单").GroupBy(a => a.ActivityName).Select(s => new { status = s.Key, count = s.Count() }).ToListAsync();
            foreach (var item in flowInstanceInfos)
            {
                switch (item.status)
                {
                    case "客服主管审批":
                        await _hubContext.Clients.Groups("客服主管").SendAsync("OutsourcCount", "系统", item.count);
                        break;
                    case "财务审核":
                        await _hubContext.Clients.Groups("财务初审").SendAsync("OutsourcCount", "系统", item.count);
                        break;
                    case "总经理审批":
                        await _hubContext.Clients.Groups("总经理").SendAsync("OutsourcCount", "系统", item.count);
                        break;
                    case "财务支付":
                        await _hubContext.Clients.Groups("出纳").SendAsync("OutsourcCount", "系统", item.count);
                        break;
                }
            }

            #endregion
            #region 售前

            var BeforeSaleFlowInstanceIds = await UnitWork.Find<BeforeSaleDemand>(c => c.Status >= 1 && c.Status <= 12).Select(a => a.FlowInstanceId).ToListAsync();
            var BeforeSale = await UnitWork.Find<FlowInstance>(a => BeforeSaleFlowInstanceIds.Contains(a.Id)).Select(a => a.MakerList).ToListAsync();

            List<string> BeforeSaleUser = new List<string>();
            foreach (var item in BeforeSale)
            {
                BeforeSaleUser.AddRange(item.Split(","));
            }
            var BeforeSaleCount = BeforeSaleUser.GroupBy(a => a).Select(a => new { Id = a.Key, Count = a.Count() });

            BeforeSaleUser = BeforeSaleUser.Distinct().ToList();
            var BeforeSaleUsers = await UnitWork.Find<User>(a => BeforeSaleUser.Contains(a.Id)).ToListAsync();
            foreach (var item in BeforeSaleCount)
            {
                var name = BeforeSaleUsers.Where(a => a.Id == item.Id).FirstOrDefault()?.Name;
                if (!string.IsNullOrEmpty(name))
                {
                    await _hubContext.Clients.User(name).SendAsync("BeforeSaleCount", "系统", item.Count);
                }
            }

            //foreach (var item in BeforeSaleInfo)
            //{
            //    switch (item.Id)
            //    {
            //        case "销售总助审批":
            //            await _hubContext.Clients.Groups("需求反馈审批-销售总助").SendAsync("BeforeSaleCount", "系统", item.Count);
            //            break;
            //        case "需求组提交需求":
            //            await _hubContext.Clients.Groups("需求反馈审批-需求工程师").SendAsync("BeforeSaleCount", "系统", item.Count);
            //            break;
            //        case "研发总助审批":
            //            await _hubContext.Clients.Groups("需求反馈审批-研发总助").SendAsync("BeforeSaleCount", "系统", item.Count);
            //            break;
            //        case "研发确认":
            //            await _hubContext.Clients.Groups("需求反馈审批-研发工程师").SendAsync("BeforeSaleCount", "系统", item.Count);
            //            break;
            //        case "总经理审批":
            //            await _hubContext.Clients.Groups("总经理").SendAsync("BeforeSaleCount", "系统", item.Count);
            //            break;
            //        case "立项":
            //            await _hubContext.Clients.Groups("需求反馈审批-研发总助").SendAsync("BeforeSaleCount", "系统", item.Count);
            //            break;
            //        case "需求提交":
            //            await _hubContext.Clients.Groups("需求反馈审批-需求工程师").SendAsync("BeforeSaleCount", "系统", item.Count);
            //            break;
            //        case "研发提交":
            //            await _hubContext.Clients.Groups("需求反馈审批-研发工程师").SendAsync("BeforeSaleCount", "系统", item.Count);
            //            break;
            //        case "测试提交":
            //            await _hubContext.Clients.Groups("需求反馈审批-测试工程师").SendAsync("BeforeSaleCount", "系统", item.Count);
            //            break;
            //        //case "实施提交":
            //        //    await _hubContext.Clients.Groups("456").SendAsync("OutsourcCount", "系统", item.Count);
            //        //    break;
                   
            //    }
            //}
            #endregion

            //推送版本号
            await _hubContext.Clients.All.SendAsync("Version", "系统", _appConfiguration.Value.Version);
        }

        /// <summary>
        /// 推送
        /// </summary>
        /// <returns></returns>
        public async Task SendPendNum()
        {
            try
            {
                #region 销售-提交给我的
                //获取提交给我的
                var wfaJobList = await UnitWork.Find<wfa_job>(r => (r.job_type_id == 13 || r.job_type_id == 7 || r.job_type_id == 1 || r.job_type_id == 72) && (r.job_state == 1 || r.job_state == 4) && r.step_id != 0).GroupBy(r => new { r.step_id }).Select(r => r.Key.step_id ).ToListAsync();

                //获取对应步骤的审批人Id
                List<int> wfaobjs = await UnitWork.Find<wfa_obj>(r => r.audit_obj_id != 0 && wfaJobList.Contains((int)r.step_id)).GroupBy(r => new { r.audit_obj_id}).Select(r => r.Key.audit_obj_id).ToListAsync();

                //查询数量
                int rowCount = 0;
                string filtequery = "job_id:`job_type_nm:`job_state:`job_nm:`customer:`applicator:`remarks:`base_entry:";
                var userNames = await UnitWork.Find<base_user>(r => wfaobjs.Contains(Convert.ToInt32(r.user_id))).Select(r => new { r.user_id, r.user_nm}).ToListAsync();
                OrderSubmtToMeReq orderSubmtToMeReq = new OrderSubmtToMeReq();
                foreach (int item in wfaobjs)
                {          
                    DataTable dt = _orderWorkbenchApp.GetSubmtToMe(1, 1, orderSubmtToMeReq, "upd_dt", "desc", item, "13☉7☉1☉72", "", "", "", "", "", true, true, out rowCount);
                    if (userNames != null && userNames.Count() > 0)
                    {
                        string userName = userNames.Where(r => r.user_id == item).Select(r => r.user_nm).FirstOrDefault();
                        if (!string.IsNullOrEmpty(userName))
                        {
                           await _hubContext.Clients.User(userName).SendAsync("SaleSubCount", "系统", rowCount);
                        }
                    }
                }
                #endregion

                #region 订单合同-提交给我的
                //获取订单合同申请单
                List<string> statusList = new List<string>() { "3", "4", "5", "8", "9", "10", "12" };
                var contracts = await UnitWork.Find<ContractApply>(r => statusList.Contains(r.ContractStatus)).GroupBy(r => new { r.ContractStatus }).Select(r => new { statu = r.Key.ContractStatus, count = r.Count() }).ToListAsync();

                //订单合同循环推送
                int fwCount = 0;
                int sqCount = 0;
                int zzCount = 0;
                foreach (var item in contracts)
                {
                    switch (item.statu)
                    {
                        case "3":
                            fwCount = fwCount + item.count;
                            break;
                        case "4":
                            fwCount = fwCount + item.count;
                            sqCount = sqCount + item.count;
                            break;
                        case "5":
                            zzCount = zzCount + item.count;
                            break;
                        case "8":
                            sqCount = sqCount + item.count;
                            break;
                        case "9":
                            fwCount = fwCount + item.count;
                            break;
                        case "10":
                            fwCount = fwCount + item.count;
                            zzCount = zzCount + item.count;
                            break;
                        case "12":
                            sqCount = sqCount + item.count;
                            break;
                    }
                }

                await _hubContext.Clients.Groups("销售总助").SendAsync("ZZContractSendCount", "系统", zzCount);
                await _hubContext.Clients.Groups("法务人员").SendAsync("FWContractSendCount", "系统", fwCount);
                await _hubContext.Clients.Groups("售前工程师").SendAsync("SQContractSendCount", "系统", sqCount);
                #endregion
            }
            catch (Exception ex)
            {
                _logger.LogError("推送提交给我的异常：" + ex.Message.ToString());
            }
        }


        /// <summary>
        /// Web获取消息个数
        /// </summary>
        /// <returns></returns>
        public async Task GetMessageWeb()
        {
            var Messages = from a in UnitWork.Find<ServiceOrderMessage>(null)
                           join b in UnitWork.Find<ServiceOrderMessageUser>(null) on a.Id equals b.MessageId into ab
                           from b in ab.DefaultIfEmpty()
                           join c in UnitWork.Find<ServiceOrder>(null) on a.ServiceOrderId equals c.Id
                           where a.CreateTime>=DateTime.Now.AddMonths(-1) && b.FromUserId != "0" && b.FroUserId!=null
                           select new { a, b, c };
            var MessageList = await Messages.Select(m => new { content=m.a.Content, serviceOrderId = m.a.ServiceOrderId, replier = m.a.Replier, createTime = m.a.CreateTime.ToString(), froUserId = m.b.FroUserId, u_SAP_ID = m.c.U_SAP_ID, hasRead = m.b.HasRead , vestInOrg=m.c.VestInOrg}).ToListAsync();
            var DistinctMessage = MessageList.OrderByDescending(m => m.createTime).GroupBy(m => new { m.serviceOrderId, m.froUserId }).Select(m => m.First()).ToList();
            var AppUserIds = DistinctMessage.Select(m => m.froUserId).Distinct().ToList();
            var AppUserMaps = await UnitWork.Find<AppUserMap>(u => AppUserIds.Contains(u.AppUserId.ToString())).Include(u => u.User).ToListAsync();
            var UserNames = AppUserMaps.Select(u => new { u.User.Name, u.AppUserId }).ToList();
            foreach (var item in UserNames)
            {
                var Message = DistinctMessage.Where(m => m.froUserId.Equals(item.AppUserId.ToString())).ToList();
                await _hubContext.Clients.User(item.Name).SendAsync("ServiceOrderMessage", "消息", Message);
            }
        }
        #endregion
    }
}
