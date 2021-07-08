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
    /// 版本日志操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "General")]
    public class VersionsLogsController : ControllerBase
    {
        private readonly VersionsLogApp _app;


        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Add(AddOrUpdateVersionsLogReq obj)
        {
            var result = new Response();
            await _app.Add(obj);
            return result;
        }

       

        /// <summary>
        /// 加载列表
        /// </summary>
        [HttpGet]
        public async Task<TableData> Load([FromQuery] PageReq request)
        {
            return await _app.Load(request);
        }
        /// <summary>
        /// 获取版本号
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> VersionsNumberList() 
        {
            return await _app.VersionsNumberList();
        }
        /// <summary>
        /// 删除
        /// </summary>
        [HttpPost]
        public async Task<Response> Delete(AddOrUpdateVersionsLogReq obj)
        {
            var result = new Response();
            await _app.Delete(obj);
            return result;
        }

        public VersionsLogsController(VersionsLogApp app) 
        {
            _app = app;
        }
    }
}
