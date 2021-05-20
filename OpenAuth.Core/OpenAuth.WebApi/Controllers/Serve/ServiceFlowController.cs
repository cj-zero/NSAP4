using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Serve.Request;
using Serilog;

namespace OpenAuth.WebApi.Controllers.Serve
{
    /// <summary>
    /// 售后流程
    /// </summary>
    [Route("api/serve/[controller]/[action]")]
    [ApiController]
    public class ServiceFlowController : ControllerBase
    {
        private readonly ServiceFlowApp _serviceFlowApp;

        public ServiceFlowController(ServiceFlowApp serviceFlowApp)
        {
            _serviceFlowApp = serviceFlowApp;
        }

        /// <summary>
        /// 添加售后流程
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> AddServerFlow(AddOrUpdateServerFlowReq request)
        {
            var result = new Response();
            try
            {
                await _serviceFlowApp.AddOrUpdateServerFlow(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }
    }
}
