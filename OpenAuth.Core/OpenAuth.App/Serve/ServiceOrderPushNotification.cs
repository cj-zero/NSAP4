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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Serve
{
    public class ServiceOrderPushNotification: OnlyUnitWorkBaeApp
    {
        private readonly IHubContext<MessageHub> _hubContext;
        private IOptions<AppSetting> _appConfiguration;

        public ServiceOrderPushNotification(IUnitWork unitWork, IHubContext<MessageHub> hubContext, IAuth auth, IOptions<AppSetting> appConfiguration) : base(unitWork, auth)
        {
            _hubContext = hubContext;
            _appConfiguration=appConfiguration;
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
            #endregion
            #region 报销单
            var reimburseInfos =await UnitWork.Find<ReimburseInfo>(r => r.RemburseStatus > 3 && r.RemburseStatus < 9).GroupBy(r=>r.RemburseStatus).Select(r=> new{ status=r.Key, count=r.Count()}).ToListAsync();
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
            //推送版本号
            await _hubContext.Clients.All.SendAsync("Version", "系统", _appConfiguration.Value.Version);
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
