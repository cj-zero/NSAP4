using System;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 实验室资产操作记录
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AssetOperationsController : ControllerBase
    {
        private readonly AssetoperationApp _app;
        
        ////获取详情
        //[HttpGet]
        //public Response<AssetOperation> Get(string id)
        //{
        //    var result = new Response<AssetOperation>();
        //    try
        //    {
        //        result.Result = _app.Get(id);
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Code = 500;
        //        result.Message = ex.InnerException?.Message ?? ex.Message;
        //    }

        //    return result;
        //}

        /// <summary>
        /// 加载列表
        /// </summary>
        [HttpGet]
        public TableData Load(string AssetId)
        {
            return _app.Load(AssetId);
        }

        public AssetOperationsController(AssetoperationApp app) 
        {
            _app = app;
        }
    }
}
