using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using Serilog;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 服务评价操作
    /// </summary>
    [Route("api/Serve/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Serve")]
    public class ServiceEvaluatesController : ControllerBase
    {
        private readonly ServiceEvaluateApp _app;
        private readonly HttpClienService _httpClienService;
        public ServiceEvaluatesController(ServiceEvaluateApp app, HttpClienService httpClienService)
        {
            _app = app;
            _httpClienService = httpClienService;
        }

        //获取详情
        [HttpGet]
        public async Task<Response<ServiceEvaluate>> Get(long id)
        {
            var result = new Response<ServiceEvaluate>();
            try
            {
                result.Result = await _app.Get(id);
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
        public async Task<Response> Add(AddOrUpdateServiceEvaluateReq obj)
        {
            var result = new Response();
            try
            {
                await _app.Add(obj);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        #region 新威智能App使用 修改请告知！！！
        [HttpPost]
        public async Task<Response> AppAdd(APPAddServiceEvaluateReq req)
        {

            var result = new Response();
            try
            {
                var r = await _httpClienService.Post(req, "api/serve/ServiceOrder/AppEvaluateAdd");
                result = JsonConvert.DeserializeObject<Response>(r);
                //await _app.AppAdd(req);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        /// 加载列表
        /// </summary>
        [HttpGet]
        public async Task<TableData> AppLoad([FromQuery] QueryServiceEvaluateListReq request)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("ServiceOrderId", request.ServiceOrderId);
                parameters.Add("CustomerId", request.CustomerId);
                parameters.Add("Technician", request.Technician);
                parameters.Add("VisitPeople", request.VisitPeople);
                parameters.Add("DateFrom", request.DateFrom);
                parameters.Add("DateTo", request.DateTo);
                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/AppEvaluateLoad");
                result = JsonConvert.DeserializeObject<TableData>(r);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}， 错误：{result.Message}");
            }
            return result;
            //return await _app.AppLoad(request);
        }
        #endregion

        //修改
        [HttpPost]
        public async Task<Response> Update(AddOrUpdateServiceEvaluateReq obj)
        {
            var result = new Response();
            try
            {
                await _app.Update(obj);

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
        public TableData Load([FromQuery] QueryServiceEvaluateListReq request)
        {
            return _app.Load(request);
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        [HttpPost]
        public async Task<Response> Delete([FromBody] long[] ids)
        {
            var result = new Response();
            try
            {
                await _app.Delete(ids);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ids.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        /// 获取技术员Id列表
        /// </summary>
        /// <param name="serviceOrderId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<List<int>>> GetTechnicianAppIds(int serviceOrderId)
        {

            var result = new Response<List<int>>();
            try
            {
                result.Result = await _app.GetTechnicianAppIds(serviceOrderId);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{serviceOrderId}， 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        /// 获取技术员姓名列表
        /// </summary>
        /// <param name="serviceOrderId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetTechnicianName(int serviceOrderId)
        {

            var result = new TableData();
            try
            {
                return await _app.GetTechnicianName(serviceOrderId);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{serviceOrderId}， 错误：{result.Message}");
            }

            return result;
        }
    }
}
