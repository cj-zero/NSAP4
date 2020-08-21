using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Response;
using OpenAuth.App.Sap.Request;
using OpenAuth.App.Sap.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OpenAuth.WebApi.Controllers.Sap
{
    /// <summary>
    /// 序列号
    /// </summary>
    [Route("api/Sap/[controller]/[action]")]
    [ApiController]
    public class SerialNumberController : ControllerBase
    {
        private readonly SerialNumberApp _serialNumberApp;

        public SerialNumberController(SerialNumberApp serialNumberApp)
        {
            _serialNumberApp = serialNumberApp;
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
            }
            return result;
        }

        /// 序列号查询（App 已生成服务单）
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> AppGet([FromQuery] QueryAppSerialNumberListReq req)
        {
            var result = new TableData();
            try
            {
                result = await _serialNumberApp.AppGet(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
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
                result = await _serialNumberApp.AppFind(req);
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
