using Microsoft.AspNetCore.SignalR;
using OpenAuth.App.Interface;
using OpenAuth.App.SignalR.Request;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
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
    }
}
