using System;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 定位相关操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RealTimeLocationsController : ControllerBase
    {
        private readonly RealTimeLocationApp _realTimeLocationApp;
        public RealTimeLocationsController(RealTimeLocationApp realTimeLocationApp)
        {
            _realTimeLocationApp = realTimeLocationApp;
        }

        /// <summary>
        /// 添加定位信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Add(AddOrUpdaterealtimelocationReq req)
        {
            var result = new Response();
            try
            {
                await _realTimeLocationApp.Add(req);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }
    }
}
