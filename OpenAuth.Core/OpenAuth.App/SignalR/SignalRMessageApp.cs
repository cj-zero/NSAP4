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
        IUnitWork _unitWork;
        IAuth _auth;
        public SignalRMessageApp(IUnitWork unitWork, IAuth auth, IHubContext<MessageHub> hubContext) : base(unitWork, auth)
        {
            _unitWork = unitWork;
            _hubContext = hubContext;
            _auth = auth;
        }

        /// <summary>
        /// 给合同管理用户发送消息
        /// </summary>
        /// <param name="req">给用户发送消息实体</param>
        /// <returns></returns>
        public async Task SendContractRoleMessage(SendRoleMessageReq req)
        {
            string userName = _auth.GetUserName();
            await _hubContext.Clients.Group(req.Role).SendAsync("ContractMessage", userName, req.Message);
        }

        /// <summary>
        /// 给单个用户发消息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task SendUserMessage(SendUserMessageReq req)
        {
            string userName = _auth.GetUserName();
            await _hubContext.Clients.User(req.UserName).SendAsync("ReceiveMessage", userName, req.Message);
        }
        /// <summary>
        /// 给一个角色用户发消息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task SendRoleMessage(SendRoleMessageReq req)
        {
            string userName = _auth.GetUserName();
            await _hubContext.Clients.Groups(req.Role).SendAsync("ReceiveMessage", userName, req.Message);
        }
        /// <summary>
        /// 给所有人发消息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task SendAllUserMessage(SendAllMessageReq req)
        {
            string userName = _auth.GetUserName();
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", userName, req.Message);
        }
        public async Task SendSystemMessage(SignalRSendType type, string message, IReadOnlyList<string> sendTo = default)
        {
            switch (type)
            {
                case SignalRSendType.All:
                    await _hubContext.Clients.All.SendAsync("SystemMessage", "系统", message);
                    break;
                case SignalRSendType.User:
                    if (sendTo != default)
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
    }
    public enum SignalRSendType : int
    {
        All = 1,
        User = 2,
        Role = 3
    }
}
