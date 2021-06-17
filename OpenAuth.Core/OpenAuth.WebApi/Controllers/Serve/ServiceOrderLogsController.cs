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
    /// 服务单日志操作
    /// </summary>
    [Route("api/serve/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Serve")]
    public class ServiceOrderLogsController : ControllerBase
    {
        private readonly ServiceOrderLogApp _app;
        
        //获取详情
        [HttpGet]
        public Response<ServiceOrderLog> Get(string id)
        {
            var result = new Response<ServiceOrderLog>();
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
        public Response Add(AddOrUpdateServiceOrderLogReq obj)
        {
            var result = new Response();
            try
            {
                _app.Add(obj);

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
        public Response Update(AddOrUpdateServiceOrderLogReq obj)
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
        public async Task<TableData> Load([FromQuery]QueryServiceOrderLogListReq request)
        {
            return  await _app.Load(request);
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

        public ServiceOrderLogsController(ServiceOrderLogApp app) 
        {
            _app = app;
        }

        /// <summary>
        /// 根据服务单id获取日志信息
        /// </summary>
        /// <param name="ServiceOrderId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetServiceOrderLog(int ServiceOrderId)
        {
            var result = new TableData();
            try
            {
                return await _app.GetServiceOrderLog(ServiceOrderId);
            }
            catch (Exception e)
            {

                result.Code = 500;
                result.Message = e.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ServiceOrderId}， 错误：{result.Message}");
            }
            return result;
        }
        
    }
}
