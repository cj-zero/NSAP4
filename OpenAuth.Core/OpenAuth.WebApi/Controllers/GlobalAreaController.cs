using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 地图
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GlobalAreaController : ControllerBase
    {
        private readonly GlobalAreaApp _app;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        public GlobalAreaController(GlobalAreaApp app)
        {
            _app = app;
        }

        /// <summary>
        /// 加载地图
        /// </summary>
        [HttpGet]
        public async Task<TableData> Load([FromQuery] QueryGlobalAreaListReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.Load(request);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 搜索地图
        /// </summary>
        [HttpGet]
        public async Task<TableData> GetArea([FromQuery] QueryGlobalAreaListReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.GetArea(request);
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
