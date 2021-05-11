using System;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using Serilog;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 模块工作流程绑定操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ModuleFlowSchemesController : ControllerBase
    {
        private readonly ModuleFlowSchemeApp _app;
        
        //获取详情
        [HttpGet]
        public Response<ModuleFlowScheme> Get(string id)
        {
            var result = new Response<ModuleFlowScheme>();
            try
            {
                result.Result = _app.Get(id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{id}， 错误：{result.Message}");
            }

            return result;
        }

        //添加
       [HttpPost]
        public async Task<Response> Add(AddOrUpdateModuleFlowSchemeReq obj)
        {
            var result = new Response();
            try
            {
                var exist = await _app.CheckExistAsync(m => m.ModuleId.Equals(obj.ModuleId));
                if (exist)
                    throw new Exception("当前模块已绑定流程，每个模块只能绑定一个流程");
                await _app.AddAsync(obj);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        //修改
       [HttpPost]
        public Response Update(AddOrUpdateModuleFlowSchemeReq obj)
        {
            var result = new Response();
            try
            {
                _app.Update(obj);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 加载列表
        /// </summary>
        [HttpGet]
        public TableData Load([FromQuery]QueryModuleFlowSchemeListReq request)
        {
            return _app.Load(request);
        }

        /// <summary>
        /// 批量删除
        /// </summary>
       [HttpPost]
        public Response Delete([FromBody]string[] ids)
        {
            var result = new Response();
            try
            {
                _app.Delete(ids);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ids.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        public ModuleFlowSchemesController(ModuleFlowSchemeApp app) 
        {
            _app = app;
        }
    }
}
