﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OpenAuth.WebApi.Controllers.Serve
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AttendanceClockController : Controller
    {
        private readonly AttendanceClockApp _app;

        public AttendanceClockController(AttendanceClockApp app)
        {
            _app = app;
        }

        /// <summary>
        /// 加载列表
        /// </summary>
        [HttpGet]
        public TableData Load([FromQuery]QueryAttendanceClockListReq request)
        {
            return _app.Load(request);
        }
        //获取详情
        [HttpGet]
        public async Task<Response<AttendanceClockDetailsResp>> Get(string id)
        {
            var result = new Response<AttendanceClockDetailsResp>();
            try
            {
                result.Result = await _app.GetDetails(id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }
    }
}