﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Response;
using OpenAuth.App.Workbench;
using OpenAuth.App.Workbench.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Workbench
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Workbench")]
    public class PendingController : ControllerBase
    {
        private readonly PendingApp _app;
        [HttpGet]
        public async Task<TableData> Load([FromQuery]PendingReq req) 
        {
            var result = new TableData();
            try
            {
                result = await _app.Load(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }

            return result;
        }
        /// <summary>
        /// 获取待处理订单详情
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> PendingDetails([FromQuery] PendingReq req) 
        {
            return await _app.PendingDetails(req);
        }
        public PendingController(PendingApp app)
        {
            _app = app;
        }
    }
}
