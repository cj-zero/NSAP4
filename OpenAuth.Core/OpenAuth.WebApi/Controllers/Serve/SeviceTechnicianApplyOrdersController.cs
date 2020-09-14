﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Serve.Request;
using OpenAuth.Repository.Domain;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 技术员提交设备操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SeviceTechnicianApplyOrdersController : ControllerBase
    {
        private readonly SeviceTechnicianApplyOrdersApp _app;
        private IOptions<AppSetting> _appConfiguration;
        private readonly HttpHelper _helper;

        public SeviceTechnicianApplyOrdersController(SeviceTechnicianApplyOrdersApp app, IOptions<AppSetting> appConfiguration)
        {
            _app = app;
            _appConfiguration = appConfiguration;
            _helper = new HttpHelper(_appConfiguration.Value.AppServerUrl);
        }

        #region 新威智能App 售后接口 如修改请告知！！
        /// <summary>
        /// 提交核对错误(新)设备信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> ApplyNewOrErrorDevices(ApplyNewOrErrorDevicesReq request)
        {
            var result = new Response();
            try
            {
                var r = _helper.Post(request, "api/serve/ServiceOrder/ApplyNewOrErrorDevices", Request.Headers["X-Token"].ToString());
                result = JsonConvert.DeserializeObject<Response>(r);
                //await _app.ApplyNewOrErrorDevices(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        /// <summary>
        ///获取技术员提交/修改的设备信息(APP)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetApplyDevices([FromQuery] GetApplyDevicesReq request)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("TechnicianId", request.TechnicianId);
                parameters.Add("ServiceOrderId", request.ServiceOrderId);
                parameters.Add("MaterialType", request.MaterialType);
                parameters.Add("X-Token", Request.Headers["X-Token"].ToString());
                var r = _helper.Get(parameters, "api/serve/ServiceOrder/GetApplyDevices");
                result = JsonConvert.DeserializeObject<TableData>(r);
                //result = await _app.GetApplyDevices(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        #endregion

        /// <summary>
        /// 服务台处理技术员提交的设备信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> SolveTechApplyDevices(SolveTechApplyDevicesReq req)
        {
            var result = new Response();
            try
            {
                await _app.SolveTechApplyDevices(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        /// <summary>
        ///获取技术员提交/修改的设备信息
        /// </summary>
        /// <param name="sapOrderId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetTechnicianApplyDevices(int sapOrderId)
        {
            var result = new TableData();
            try
            {
                result = await _app.GetTechnicianApplyDevices(sapOrderId);
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
