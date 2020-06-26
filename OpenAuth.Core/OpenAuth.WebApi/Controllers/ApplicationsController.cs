using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using Org.BouncyCastle.Ocsp;
using System;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 应用管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ApplicationsController : ControllerBase
    {
        private readonly AppManager _app;

        public ApplicationsController(AppManager app) 
        {
            _app = app;
        }
        /// <summary>
        /// 加载应用列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> Load([FromQuery]QueryAppListReq request)
        {
            var applications = await _app.GetPageAsync(request);
            return new TableData
            {
                data = applications,
                count = applications.Count
            };
        }
        /// <summary>
        /// 新增或更新应用信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> AddOrUpdate([FromBody]AddOrUpdateAppReq request)
        {
            var result = new Response();
            try
            {
                if (string.IsNullOrWhiteSpace(request.Id))
                {
                    await _app.AddAsync(request);
                }
                else
                {
                    await _app.UpdateAsync(request);
                }
            }catch(Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

    }
}