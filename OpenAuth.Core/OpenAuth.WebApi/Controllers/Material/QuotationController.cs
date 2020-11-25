using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Material;
using OpenAuth.App.Material.Request;
using OpenAuth.App.Response;

namespace OpenAuth.WebApi.Controllers.Material
{
    /// <summary>
    /// 报价表
    /// </summary>
    [Route("api/Material/[controller]/[action]")]
    [ApiController]
    public class QuotationController : ControllerBase
    {
        private readonly QuotationApp _app;
        public QuotationController(QuotationApp app)
        {
            _app = app;
        }

        /// <summary>
        /// 加载报价单列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> Load([FromQuery] QueryQuotationListReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.Load(request);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 加载服务单列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetServiceOrderList([FromQuery]QueryQuotationListReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.GetServiceOrderList(request);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 获取序列号和设备
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetSerialNumberList([FromQuery] QueryQuotationListReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.GetSerialNumberList(request);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }
        
        /// <summary>
        /// 加载物料列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetMaterialCodeList([FromQuery] QueryQuotationListReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.GetMaterialCodeList(request);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 报价单详情
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetDetails(int QuotationId)
        {
            var result = new TableData();
            try
            {
                return await _app.GetDetails(QuotationId);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }
        
        /// <summary>
        /// 添加报价单
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Add(AddOrUpdateQuotationReq obj) 
        {
            var result = new Response();
            try
            {
                await _app.Add(obj);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 修改报价单
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Update(AddOrUpdateQuotationReq obj)
        {
            var result = new Response();
            try
            {
                await _app.Update(obj);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 审批报价单
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Accraditation(AddOrUpdateQuotationReq obj)
        {
            var result = new Response();
            try
            {
                await _app.Accraditation(obj);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }
    }
}
