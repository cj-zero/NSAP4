using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Serve.Request;
using OpenAuth.Repository.Domain;
using Serilog;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// appserviceorderlog操作
    /// </summary>
    [Route("api/serve/[controller]/[action]")]
    [ApiController]
    public class AppServiceOrderLogController : ControllerBase
    {
        private readonly AppServiceOrderLogApp _app;
        
        //获取详情
        [HttpGet]
        public Response<AppServiceOrderLog> Get(string id)
        {
            var result = new Response<AppServiceOrderLog>();
            try
            {
                result.Result = _app.Get(id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{id}, 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        /// 获取服务单和工单的状态记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<List<OrderLogListResp>>> GetOrderLog([FromQuery]GetOrderLogListReq request)
        {
            var result = new Response<List<OrderLogListResp>>();
            try
            {
                result.Result = await _app.GetOrderLog(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }
        //添加
       [HttpPost]
        public async Task<Response> Add(AddOrUpdateAppServiceOrderLogReq obj)
        {
            var result = new Response();
            try
            {
                await _app.AddAsync(obj);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        //修改
       [HttpPost]
        public Response Update(AddOrUpdateAppServiceOrderLogReq obj)
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 加载列表
        /// </summary>
        [HttpGet]
        public TableData Load([FromQuery]QueryAppServiceOrderLogListReq request)
        {
            return _app.Load(request);
        }

        /// <summary>
        /// 批量删除
        /// </summary>
       [HttpPost]
        public async Task<Response> Delete([FromBody]string[] ids)
        {
            var result = new Response();
            try
            {
                await _app.DeleteAsync(ids);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ids.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 获取服务单和工单的状态记录(APP)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<Dictionary<string, object>>> GetAppOrderLogList([FromQuery] GetAppOrderLogListReq request)
        {
            var result = new Response<Dictionary<string, object>>();
            try
            {
                result.Result = await _app.GetAppOrderLogList(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        public AppServiceOrderLogController(AppServiceOrderLogApp app) 
        {
            _app = app;
        }
    }
}
