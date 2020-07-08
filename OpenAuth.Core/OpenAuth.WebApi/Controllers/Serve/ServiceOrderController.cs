using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using NetOffice.Extensions.Invoker;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 服务单
    /// </summary>
    [Route("api/serve/[controller]/[action]")]
    [ApiController]
    public class ServiceOrderController : Controller
    {
        private readonly ServiceOrderApp _serviceOrderApp;

        public ServiceOrderController(ServiceOrderApp serviceOrderApp)
        {
            _serviceOrderApp = serviceOrderApp;
        }
        /// <summary>
        /// 新增服务单
        /// </summary>
        /// <param name="addServiceOrderReq"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Add(AddServiceOrderReq addServiceOrderReq)
        {
            var result = new Response();
            try
            {
                await _serviceOrderApp.Add(addServiceOrderReq);
            }catch(Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 服务单查询
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public Task<TableData> Load([FromQuery]QueryServiceOrderListReq query)
        {
            var result = new TableData();
            try
            {
                result.data = _serviceOrderApp.Load(query);
            }catch(Exception ex)
            {
                result.code = 500;
                result.msg = ex.Message;
            }
            return Task.FromResult(result);
        }
    }
}
