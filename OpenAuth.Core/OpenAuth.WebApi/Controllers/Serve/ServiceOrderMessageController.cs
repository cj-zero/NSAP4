using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using NPOI.OpenXml4Net.OPC.Internal;
using OpenAuth.App.Request;
using OpenAuth.App.Serve;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Serve
{
    /// <summary>
    /// 客服消息
    /// </summary>
    [Route("api/serve/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Serve")]
    public class ServiceOrderMessageController : ControllerBase
    {
        private readonly ServiceOrderMessageApp _serviceOrderMessageApp;

        public ServiceOrderMessageController(ServiceOrderMessageApp serviceOrderMessageApp)
        {
            _serviceOrderMessageApp = serviceOrderMessageApp;
        }

        [HttpGet]
        public async Task<Response<dynamic>> GetServiceOrderMessages(int serviceOrderId)
        {
            var result = new Response<dynamic>();
            try
            {
                result.Result = await _serviceOrderMessageApp.GetServiceOrderMessages(serviceOrderId);
            }
            catch(Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{serviceOrderId}， 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 发消息给技术员
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> SendMessageToTechnician(SendMessageToTechnicianReq req)
        {
            var result = new Response();
            try
            {
                await _serviceOrderMessageApp.SendMessageToTechnician(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }
    }
}
