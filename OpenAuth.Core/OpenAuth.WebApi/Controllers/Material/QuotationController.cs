using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Material;
using OpenAuth.App.Material.Request;
using OpenAuth.App.Response;

namespace OpenAuth.WebApi.Controllers.Material
{
    /// <summary>
    /// 报价单操作
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
        /// 待审批列表
        /// </summary>
        [HttpGet]
        public async Task<TableData> ApprovalPendingLoad([FromQuery]  QueryQuotationListReq request) 
        {
            var result = new TableData();
            try
            {
                //return await _app.ApprovalPendingLoad(request);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }
        
        /// <summary>
        /// 获取报价单流程
        /// </summary>
        [HttpGet]
        public async Task<TableData> GetQuotationOperationHistory([FromQuery] QueryQuotationListReq request) 
        {
            var result = new TableData();
            try
            {
                return await _app.GetQuotationOperationHistory(request);
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
        /// 按条件查询所有物料
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> MaterialCodeList([FromQuery] QueryQuotationListReq request) 
        {
            var result = new TableData();
            try
            {
                return await _app.MaterialCodeList(request);
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
        /// 查询报价单详情物料
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetDetailsMaterial([FromQuery] QueryQuotationListReq request) 
        {
            var result = new TableData();
            try
            {
                return await _app.GetDetailsMaterial(request);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 获取待合并报价单
        /// </summary>
        /// <param name="ServiceOrderId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetUnreadQuotations(int ServiceOrderId) 
        {
            var result = new TableData();
            try
            {
                return await _app.GetUnreadQuotations(ServiceOrderId);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 获取该服务单所有报价单零件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetQuotationMaterialCode([FromQuery]QueryQuotationListReq request) 
        {
            var result = new TableData();
            try
            {
                return await _app.GetQuotationMaterialCode(request);
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
                var Message = await _app.Add(obj);
                if (!string.IsNullOrWhiteSpace(Message)) 
                {
                    result.Message = Message;
                }
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
                var Message = await _app.Update(obj);
                if (!string.IsNullOrWhiteSpace(Message))
                {
                    result.Message = Message;
                }
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// 撤回报价单
        /// </summary>
        /// <param name="QuotationId"></param>
        [HttpPost]
        public async Task<Response> Revocation(QueryQuotationListReq obj) 
        {
            var result = new Response();
            try
            {
                await _app.Revocation((int)obj.QuotationId);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 修改报价单物料，物流
        /// </summary>
        /// <param name="obj"></param>
        [HttpPost]
        public async Task<TableData> UpdateMaterial(AddOrUpdateQuotationReq obj)
        {
            var result = new TableData();
            try
            {
               return await _app.UpdateMaterial(obj);
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
        public async Task<Response> Accraditation(AccraditationQuotationReq req)
        {
            var result = new Response();
            try
            {
                await _app.Accraditation(req);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 删除报价单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Delete(QueryQuotationListReq req) 
        {
            var result = new Response();
            try
            {
                await _app.Delete(req);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 导入设备零件价格
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> ImportMaterialPrice()
        {
            var result = new Response();
            try
            {
                var file = Request.Form.Files[0];
                var handler = new ExcelHandler(file.OpenReadStream());
                await _app.ImportMaterialPrice(handler);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 获取合并后信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetMergeMaterial([FromQuery]QueryQuotationListReq req) 
        {
            var result = new TableData();
            try
            {
                return await _app.GetMergeMaterial(req);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 打印销售订单
        /// </summary>
        /// <param name="QuotationId"></param>
        /// <returns></returns>
        [HttpGet("{QuotationId}")]
        public async Task<IActionResult> PrintSalesOrder(string QuotationId)
        {
            try
            {
                return File(await _app.PrintSalesOrder(QuotationId), "application/pdf");
            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }
           
        }
        /// <summary>
        /// 打印报价单
        /// </summary>
        /// <param name="QuotationId"></param>
        /// <returns></returns>
        [HttpGet("{QuotationId}")]
        public async Task<IActionResult> PrintQuotation(string QuotationId)
        {
            try
            {
                return File(await _app.PrintQuotation(QuotationId), "application/pdf");
            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 打印领料单
        /// </summary>
        /// <param name="QuotationId"></param>
        /// <returns></returns>
        [HttpGet("{QuotationId}")]
        public async Task<IActionResult> PrintPickingList(string QuotationId)
        {
            try
            {
                return File(await _app.PrintPickingList(QuotationId), "application/pdf");
            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }

        }
        
    }
}
