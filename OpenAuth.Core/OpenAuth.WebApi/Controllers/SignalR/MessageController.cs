using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OpenAuth.App.SignalR;
using OpenAuth.App.SignalR.Request;

namespace OpenAuth.WebApi.Controllers.SignalR
{
    [Route("api/SignalR/[controller]/[action]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly SignalRMessageApp _messageApp;

        public MessageController(SignalRMessageApp messageApp)
        {
            _messageApp = messageApp;
        }

        /// <summary>
        /// 给一个角色用户发送消息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> SendRoleMessage(SendRoleMessageReq req)
        {
            var result = new Response();
            try
            {
                await _messageApp.SendRoleMessage(req);
            }
            catch(Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }
       /// <summary>
       /// 给所有人发送 消息
       /// </summary>
       /// <param name="req"></param>
       /// <returns></returns>
        [HttpPost]
        public async Task<Response> SendAllUserMessage(SendAllMessageReq req)
        {
            var result = new Response();
            try
            {
                await _messageApp.SendAllUserMessage(req);
            }
            catch(Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 给一个用户发送消息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> SendUserMessage(SendUserMessageReq req)
        {
            var result = new Response();
            try
            {
                await _messageApp.SendUserMessage(req);
            }
            catch(Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }
    }
}
