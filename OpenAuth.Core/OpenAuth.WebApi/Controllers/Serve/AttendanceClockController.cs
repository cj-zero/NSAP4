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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 考勤打卡
    /// </summary>
    [Route("api/serve/[controller]/[action]")]
    [ApiController]
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
        [HttpGet]
        public TableData Load([FromQuery] QueryAttendanceClockListReq request)
        {
            return _app.Load(request);
        }

        /// <summary>
        /// 导出考勤Excel
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ExportAttendanceClock([FromQuery] QueryAttendanceClockListReq req)
        {
            var data = await _app.ExportAttendanceClock(req);

            return File(data, "application/vnd.ms-excel");
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
            }

            return result;
        }
        #endregion
    }
}
