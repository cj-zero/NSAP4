using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenAuth.App;
using OpenAuth.App.Response;
using OpenAuth.App.Sap.Request;
using OpenAuth.App.Sap.Service;
using Serilog;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OpenAuth.WebApi.Controllers.Sap
{
    /// <summary>
    /// 序列号
    /// </summary>
    [Route("api/Sap/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Sap")]
    public class SerialNumberController : ControllerBase
    {
        private readonly SerialNumberApp _serialNumberApp;
        private readonly HttpClienService _httpClienService;

        public SerialNumberController(SerialNumberApp serialNumberApp, HttpClienService httpClienService)
        {
            _serialNumberApp = serialNumberApp;
            _httpClienService = httpClienService;
        }

        /// <summary>
        /// 序列号查询
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> Get([FromQuery] QuerySerialNumberListReq req)
        {
            var result = new TableData();
            try
            {
                result = await _serialNumberApp.Find(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }

        /// 序列号查询（App 已生成服务单）
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> AppGet(QueryAppSerialNumberListReq req)
        {
            var result = new TableData();
            try
            {
                var r = await _httpClienService.Post(req, "api/serve/ServiceOrder/AppSerialNumberGet");
                result = JsonConvert.DeserializeObject<TableData>(r);
                //result = await _serialNumberApp.AppGet(req);
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
        /// 序列号查询
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> AppFind(QueryAppSerialNumberListReq req)
        {
            var result = new TableData();
            try
            {
                var r = await _httpClienService.Post(req, "api/serve/ServiceOrder/AppSerialNumberFind");
                result = JsonConvert.DeserializeObject<TableData>(r);
                //result = await _serialNumberApp.AppFind(req);
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
        /// 服务呼叫序列号列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetContractList([FromQuery] QuerySerialNumberListReq req)
        {
            var result = new TableData();
            try
            {
                result = await _serialNumberApp.GetContractList(req);
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
        /// 根据客户代码获取已购买的设备信息
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetEquipments([FromQuery] QuerySerialNumberListReq request)
        {
            var result = new TableData();
            try
            {
                result = await _serialNumberApp.GetEquipments(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }
        [HttpGet]
        public async Task<TableData> GetSerialNoAndXWJVersionByDeliveryNo(string DeliveryNo)
        {
            var result = new TableData();
            try
            {
                result = await _serialNumberApp.GetSerialNoAndXWJVersionByDeliveryNo(DeliveryNo);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{DeliveryNo}, 错误：{result.Message}");
            }
            return result;
        }
    }
}
