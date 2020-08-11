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
    /// 实验室资产送检操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AssetInspectsController : ControllerBase
    {
        private readonly AssetinspectApp _app;
        
        ////获取详情
        //[HttpGet]
        //public Response<AssetInspect> Get(string id)
        //{
        //    var result = new Response<AssetInspect>();
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

        public AssetInspectsController(AssetinspectApp app) 
        {
            _app = app;
        }
    }
}
