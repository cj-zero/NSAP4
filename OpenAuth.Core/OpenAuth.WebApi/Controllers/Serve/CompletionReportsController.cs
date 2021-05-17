using System;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// completionreport操作
    /// </summary>
    [Route("api/serve/[controller]/[action]")]
    [ApiController]
    public class CompletionReportsController : ControllerBase
    {
        private readonly CompletionReportApp _app;
        private readonly HttpClienService _httpClienService;

        public CompletionReportsController(CompletionReportApp app, HttpClienService httpClienService)
        {
            _app = app;
            _httpClienService = httpClienService;
        }

        //获取详情
        [HttpGet]
        public Response<CompletionReport> Get(string id)
        {
            var result = new Response<CompletionReport>();
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

        #region 新威智能APP售后接口 若修改请告知！！！
        //添加
        [HttpPost]
        public async Task<Response> Add(AddOrUpdateCompletionReportReq obj)
        {
            var result = new Response();
            try
            {
                var r = await _httpClienService.Post(obj, "api/serve/ServiceOrder/AddCompletionReport");
                result = JsonConvert.DeserializeObject<Response>(r);
                //await _app.Add(obj);
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
        /// 填写完工报告单页面需要取到的服务工单信息。
        /// </summary>
        /// <param name="ServiceOrderId">服务单ID</param>
        /// <param name="currentUserId">当前技术员Id</param>
        /// <param name="MaterialType">设备类型</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<CompletionReportDetailsResp>> GetOrderWorkInfoForAdd(int ServiceOrderId, int currentUserId, string MaterialType)
        {
            var result = new Response<CompletionReportDetailsResp>();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("ServiceOrderId", ServiceOrderId);
                parameters.Add("currentUserId", currentUserId);
                parameters.Add("MaterialType", MaterialType);
                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/GetOrderWorkInfoForAdd");
                JObject data = JsonConvert.DeserializeObject<JObject>(r);
                result.Result = JsonConvert.DeserializeObject<CompletionReportDetailsResp>(data.Property("result").Value.ToString());
                //result.Result = await _app.GetOrderWorkInfoForAdd(ServiceOrderId, currentUserId, MaterialType);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ServiceOrderId},{currentUserId},{MaterialType}, 错误：{result.Message}");
            }

            return result;
        }

        #endregion
        ////修改
        //[HttpPost]
        //public Response Update(AddOrUpdateCompletionReportReq obj)
        //{
        //    var result = new Response();
        //    try
        //    {
        //        _app.Update(obj);

        //    }
        //    catch (Exception ex)
        //    {
        //        result.Code = 500;
        //        result.Message = ex.InnerException?.Message ?? ex.Message;
        //        Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}, 错误：{result.Message}");
        //    }

        //    return result;
        //}

        //修改(E3)
        [HttpPost]
        public async Task<Response> CISEUpdate(AddOrUpdateCompletionReportReq obj)
        {
            var result = new Response();
            try
            {
                await _app.CISEUpdate(obj);

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
        public async Task<TableData> Load([FromQuery] QueryCompletionReportListReq request)
        {
            return await _app.Load(request);
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        [HttpPost]
        public Response Delete([FromBody] string[] ids)
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{ids.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 获取完工报告详情
        /// </summary>
        /// <param name="serviceOrderId">服务单Id</param>
        /// <param name="currentUserId">当前技术员Id</param>
        /// <param name="MaterialType">设备类型</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<CompletionReportDetailsResp>> GetCompletionReportDetails(int serviceOrderId, int currentUserId, string MaterialType)
        {
            var result = new Response<CompletionReportDetailsResp>();
            try
            {
                result.Result = await _app.GetCompletionReportDetails(serviceOrderId, currentUserId, MaterialType);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{serviceOrderId},{currentUserId},{MaterialType}, 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        /// 工程部提交完工报告
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> CISEAdd(AddOrUpdateCompletionReportReq obj)
        {
            var result = new Response();
            try
            {
                await _app.CISEAdd(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }
        /// <summary>
        /// 获取完工报告详情Web
        /// </summary>
        /// <param name="ServiceOrderId"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>

        [HttpGet]
        public async Task<Response<TableData>> GetCompletionReportDetailsWeb(int ServiceOrderId,string UserId)
        {
            var result = new Response<TableData>();
            try
            {
                result.Result = await _app.GetCompletionReportDetailsWeb(ServiceOrderId, UserId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ServiceOrderId},{UserId}， 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        /// 添加完工报告Web
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        //[HttpPost]
        //public async Task<Response> AddWeb(AddOrUpdateCompletionReportReq obj)
        //{
        //    var result = new Response();
        //    try
        //    {
        //        await _app.AddWeb(obj);
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Code = 500;
        //        result.Message = ex.InnerException?.Message ?? ex.Message;
        //    }

        //    return result;
        //}

        ///// <summary>
        ///// 解除绑定
        ///// </summary>
        ///// <param name="ServiceOrderId"></param>
        ///// <param name="MaterialType"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public async Task<bool> UnbindProtectPhone(int ServiceOrderId, string MaterialType)
        //{
        //    bool IsSuccess;
        //    try
        //    {
        //        IsSuccess = await _app.UnbindProtectPhone(ServiceOrderId, MaterialType);
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //    return IsSuccess;
        //}
    }
}
