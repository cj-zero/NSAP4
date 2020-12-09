﻿using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ExpressageController : Controller
    {
        private readonly ExpressageApp _app;

        public ExpressageController(ExpressageApp app)
        {
            _app = app;
        }

        /// <summary>
        /// 查询物流信息
        /// </summary>
        [HttpGet]
        public async Task<TableData> GetExpressInfo(string trackNumber)
        {
            var result = new TableData();
            try
            {
                result = await _app.GetExpressInfo(trackNumber);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 批量查询物流信息
        /// </summary>
        [HttpPost]
        public async Task<TableData> BatchGetExpressInfo(List<string> trackNumbers)
        {
            var result = new TableData();
            try
            {
                result = await _app.BatchGetExpressInfo(trackNumbers);
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