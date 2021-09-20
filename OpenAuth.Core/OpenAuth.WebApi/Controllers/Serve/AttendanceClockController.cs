using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Serve.Request;
using Serilog;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 考勤打卡
    /// </summary>
    [Route("api/serve/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Serve")]
    public class AttendanceClockController : Controller
    {
        private readonly AttendanceClockApp _app;
        private readonly HttpClienService _httpClienService;
        public AttendanceClockController(AttendanceClockApp app, HttpClienService httpClienService)
        {
            _app = app;
            _httpClienService = httpClienService;
        }

        /// <summary>
        /// 加载列表
        /// </summary>
        [HttpPost]
        public TableData Load([FromBody] QueryAttendanceClockListReq request)
        {
            return _app.Load(request);
        }

        /// <summary>
        /// 导出考勤Excel
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ExportAttendanceClock([FromBody] QueryAttendanceClockListReq req)
        {
            var data = await _app.ExportAttendanceClock(req);

            return File(data, "application/vnd.ms-excel");
        }

        /// <summary>
        /// 获取打卡推送名单
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> LoadWhiteList()
        {
            return await _app.LoadWhiteList();
        }

        /// <summary>
        /// 配置打卡推送名单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> AddWhiteList(AddWhiteListReq request)
        {
            var result = new Response();
            try
            {
                await _app.AddWhiteList(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        #region 新威智能APP售后接口 若修改请告知！！！
        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<AttendanceClockDetailsResp>> Get(string id)
        {
            var result = new Response<AttendanceClockDetailsResp>();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("id", id);
                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/GetAttendanceClockDetail");
                JObject data = JsonConvert.DeserializeObject<JObject>(r);
                result.Result = JsonConvert.DeserializeObject<AttendanceClockDetailsResp>(data.Property("result").Value.ToString());
                //result.Result = await _app.GetDetails(id);
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
        /// App技术员当天签到和签退
        /// </summary>
        /// <param name="AppUserId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> AppGetClockCurrentHistory(int AppUserId)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("AppUserId", AppUserId);
                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/AppGetClockCurrentHistory");
                result = JsonConvert.DeserializeObject<TableData>(r);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{AppUserId}, 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        /// 打卡
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Clock(AddOrUpdateAttendanceClockReq req)
        {
            var result = new Response();
            try
            {
                var r = await _httpClienService.Post(req, "api/serve/ServiceOrder/Clock");
                result = JsonConvert.DeserializeObject<Response>(r);
                //await _app.Add(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// App技术员查询打卡记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> AppGetClockHistory([FromQuery] AppGetClockHistoryReq req)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("AppUserId", req.AppUserId);
                parameters.Add("limit", req.limit);
                parameters.Add("page", req.page);
                parameters.Add("key", req.key);
                var r = await _httpClienService.Get(parameters, "api/serve/ServiceOrder/AppGetClockHistory");
                result = JsonConvert.DeserializeObject<TableData>(r);
                //result = await _app.AppGetClockHistory(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }
        #endregion
    }
}
