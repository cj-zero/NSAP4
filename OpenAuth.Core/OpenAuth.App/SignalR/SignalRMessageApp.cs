using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Response;
using OpenAuth.App.SignalR.Request;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.SignalR
{
    public class SignalRMessageApp : OnlyUnitWorkBaeApp
    {
        private readonly IHubContext<MessageHub> _hubContext;
        public SignalRMessageApp(IUnitWork unitWork, IAuth auth, IHubContext<MessageHub> hubContext) : base(unitWork, auth)
        {
            _hubContext = hubContext;
        }
        /// <summary>
        /// 给单个用户发消息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task SendUserMessage(SendUserMessageReq req)
        {
            await _hubContext.Clients.User(req.UserName).SendAsync("ReceiveMessage", _auth.GetUserName(), req.Message);
        }
        /// <summary>
        /// 给一个角色用户发消息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task SendRoleMessage(SendRoleMessageReq req)
        {
            await _hubContext.Clients.Groups(req.Role).SendAsync("ReceiveMessage", _auth.GetUserName(), req.Message);
        }
        /// <summary>
        /// 给所有人发消息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task SendAllUserMessage(SendAllMessageReq req)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", _auth.GetUserName(), req.Message);
        }

        /// <summary>
        /// 推送待处理呼叫服务数量和待派单数量
        /// </summary>
        /// <returns></returns>
        public async Task SendPendingNumber()
        {
            await _hubContext.Clients.Groups("呼叫中心").SendAsync("ServiceOrderCount", "系统", await GetServiceOrderCount());

            var data = await GetServiceWorkOrderCount();

            foreach (var item in data)
            {
                await _hubContext.Clients.User(item.Key).SendAsync("ServiceWordOrderCount", "系统",item.Count());
            }
            
        }
        
        public async Task SendSystemMessage(SignalRSendType type, string message, IReadOnlyList<string> sendTo = default)
        {
            switch (type)
            {
                case SignalRSendType.All:
                    await _hubContext.Clients.All.SendAsync("SystemMessage", "系统", message);
                    break;
                case SignalRSendType.User:
                    if(sendTo != default)
                        await _hubContext.Clients.Users(sendTo).SendAsync("SystemMessage", "系统", message);
                    break;
                case SignalRSendType.Role:
                    if (sendTo != default)
                        await _hubContext.Clients.Groups(sendTo).SendAsync("SystemMessage", "系统", message);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 获取未处理服务单总数
        /// </summary>
        /// <returns></returns>
        private async Task<int> GetServiceOrderCount() 
        {
            return await UnitWork.Find<ServiceOrder>(u => u.Status == 1).CountAsync();
        }
        /// <summary>
        /// 获取未派单工单总数
        /// </summary>
        /// <returns></returns>
        private async Task<List<IGrouping<string, ServiceOrder>>> GetServiceWorkOrderCount()
        {
            var result = new TableData();
            var model = UnitWork.Find<ServiceWorkOrder>(s => s.Status == 1).Select(s => s.ServiceOrderId).Distinct();
            var ids = await model.ToListAsync();
            var query = await UnitWork.Find<ServiceOrder>(s => ids.Contains(s.Id)).ToListAsync();
            var groub = query.GroupBy(s => s.Supervisor).ToList();
            return groub;
        }

    }
    public enum SignalRSendType : int
    {
        All = 1,
        User = 2,
        Role = 3
    }
}
