using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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

        public ServiceOrderPushNotification(IUnitWork unitWork, IHubContext<MessageHub> hubContext, IAuth auth) : base(unitWork, auth)
        {
            _hubContext = hubContext;
        }

        #region 定时推送
        /// <summary>
        /// 推送待处理呼叫服务数量和待派单数量
        /// </summary>
        /// <returns></returns>
        public async Task SendPendingNumber()
        {
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

        }

        /// <summary>
        /// Web获取未读消息个数
        /// </summary>
        /// <returns></returns>
        public async Task GetMessageWeb()
        {
            var Messages = from a in UnitWork.Find<ServiceOrderMessage>(null)
                           join b in UnitWork.Find<ServiceOrderMessageUser>(null) on a.Id equals b.MessageId into ab
                           from b in ab.DefaultIfEmpty()
                           join c in UnitWork.Find<ServiceOrder>(null) on a.ServiceOrderId equals c.Id
                           where b.HasRead == false && b.FromUserId != "0"
                           select new { a, b, c };
            var MessageList = await Messages.Select(m => new { m.a.Content, m.a.ServiceOrderId, m.a.Replier, CreateTime = m.a.CreateTime.ToString(), m.b.FroUserId, m.c.U_SAP_ID }).ToListAsync();
            var DistinctMessage = MessageList.OrderByDescending(m => m.CreateTime).GroupBy(m => new { m.ServiceOrderId, m.FroUserId }).Select(m => m.First()).ToList();
            var AppUserIds = DistinctMessage.Select(m => m.FroUserId).Distinct().ToList();
            var AppUserMaps = await UnitWork.Find<AppUserMap>(u => AppUserIds.Contains(u.AppUserId.ToString())).Include(u => u.User).ToListAsync();
            var UserNames = AppUserMaps.Select(u => new { u.User.Name, u.AppUserId }).ToList();
            foreach (var item in UserNames)
            {
                var Message = DistinctMessage.Where(m => m.FroUserId.Equals(item.AppUserId.ToString())).ToList();
                await _hubContext.Clients.User(item.Name).SendAsync("ServiceOrderMessage", "消息", Message);
            }
        }
        #endregion
    }
}
