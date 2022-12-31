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
using OpenAuth.Repository.Domain.Material;
using OpenAuth.WebApi.Model;
using Serilog;

namespace OpenAuth.WebApi.Controllers.Material
{
    /// <summary>
    /// 报价单操作
    /// </summary>
    [Route("api/Material/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Material")]
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 待审批列表
        /// </summary>
        [HttpGet]
        public async Task<TableData> ApprovalPendingLoad([FromQuery] QueryQuotationListReq request)
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 是否有更换类型物料未退料
        /// </summary>
        [HttpGet]
        public async Task<Response<bool>> IsReturnMaterial([FromQuery] QueryQuotationListReq request)
        {
            Response<bool> result = new Response<bool>();
            try
            {
                result.Result= await _app.IsReturnMaterial(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 加载服务单列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetServiceOrderList([FromQuery] QueryQuotationListReq request)
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 查询物料剩余库存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetMaterialCodeOnHand([FromQuery] QueryQuotationListReq request)
        {

            var result = new TableData();
            try
            {
                return await _app.GetMaterialCodeOnHand(request);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }


        /// <summary>
        /// 获取物料仓库与库存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetMaterialOnHand([FromQuery] QueryQuotationListReq request)
        {
            return await _app.GetMaterialOnHand(request);
        }

        /// <summary>
        /// 获取所有物料
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetMaterial([FromQuery] QueryQuotationListReq request)
        {
            return await _app.GetMaterial(request);
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }

        #region 常用物料
        /// <summary>
        /// 添加常用物料
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> AddCommonUsedMaterial(List<AddCommonUsedMaterialReq> request)
        {
            Response result = new Response();
            try
            {
                await _app.AddCommonUsedMaterial(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message},堆栈信息：{ex.StackTrace}");
            }
            return result;
        }

        /// <summary>
        /// 删除常用物料
        /// </summary>
        /// <param name="materialCode"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response> DeleteCommonUsedMaterial(string materialCode)
        {
            Response result = new Response();
            try
            {
                await _app.DeleteCommonUsedMaterial(materialCode);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{materialCode}, 错误：{result.Message},堆栈信息：{ex.StackTrace}");
            }
            return result;
        }

        /// <summary>
        /// 获取常用物料
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetCommonUsedMaterial([FromQuery] QueryQuotationListReq request)
        {
            TableData result = new TableData();
            try
            {
                result= await _app.GetCommonUsedMaterial(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message},堆栈信息：{ex.StackTrace}");
            }
            return result;
        }
        #endregion

        /// <summary>
        /// 报价单详情
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetDetails([FromQuery] QueryQuotationListReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.GetDetails(request);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{ServiceOrderId}, 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 获取该服务单所有报价单零件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetQuotationMaterialCode([FromQuery] QueryQuotationListReq request)
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}, 错误：{result.Message}");
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}, 错误：{result.Message}");
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}, 错误：{result.Message}");
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}, 错误：{result.Message}");
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 修改报价单物料，物流
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response> TimeOfDelivery(int QuotationId)
        {
            var result = new Response();
            try
            {
                await _app.TimeOfDelivery(QuotationId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{QuotationId}, 错误：{result.Message}");
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
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
                Log.Logger.Error($"地址：{Request.Path}, 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 获取合并后信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetMergeMaterial([FromQuery] QueryQuotationListReq req)
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 打印销售订单
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <param name="sign"></param>
        /// <param name="timespan"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(CertAuthFilter))]
        [HttpGet]
        public async Task<TableData> PrintSalesOrder(string serialNumber, string sign, string timespan)
        {
            try
            {
                return await _app.PrintSalesOrder(serialNumber);
            }
            catch (Exception e)
            {

                Log.Logger.Error($"地址：{Request.Path}，参数：{serialNumber},{sign},{timespan}, 错误：{e.Message}");
                throw new Exception(e.Message);
            }

        }

        /// <summary>
        /// 打印报价单
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <param name="sign"></param>
        /// <param name="timespan"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(CertAuthFilter))]
        [HttpGet]
        public async Task<IActionResult> PrintQuotation(string serialNumber, string sign, string timespan)
        {
            try
            {
                return File(await _app.PrintQuotation(serialNumber), "application/pdf");
            }
            catch (Exception e)
            {
                Log.Logger.Error($"地址：{Request.Path}，参数：{serialNumber},{sign},{timespan}, 错误：{e.Message}");
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 打印领料单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> PrintStockRequisition(List<QuotationMergeMaterialReq> req)
        {
            var result = new Response();
            try
            {
                await _app.PrintStockRequisition(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 打印领料单
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> PrintStockRequisition(string serialNumber)
        {
            try
            {
                return File(await _app.PrintStockRequisition(serialNumber), "application/pdf");
            }
            catch (Exception e)
            {
                Log.Logger.Error($"地址：{Request.Path}，参数：{serialNumber}, 错误：{e.Message}");
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// 打印装箱清单
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <param name="IsTrue"></param>
        /// <param name="sign"></param>
        /// <param name="timespan"></param>
        /// <returns></returns>
        [HttpGet]
        [ServiceFilter(typeof(CertAuthFilter))]
        public async Task<IActionResult> PrintPickingList(string serialNumber,bool? IsTrue, string sign, string timespan)
        {
            try
            {
                return File(await _app.PrintPickingList(serialNumber, IsTrue), "application/pdf");
            }
            catch (Exception e)
            {
                Log.Logger.Error($"地址：{Request.Path}，参数：{serialNumber},{IsTrue},{sign},{timespan}, 错误：{e.Message}");
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 同步销售订单
        /// </summary>
        /// <param name="QuotationId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> SyncSalesOrder(SyncSalesOrder request) 
        {
            var result = new Response();
            try
            {
                await _app.SyncSalesOrder(request.QuotationId);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.QuotationId}, 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 同步销售订单
        /// </summary>
        /// <param name="QuotationId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> SyncSalesOrderToSap(SyncSalesOrder request)
        {
            var result = new Response();
            try
            {
                await _app.SyncSalesOrderToSap(request.QuotationId);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.QuotationId}, 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 同步销售交货
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> SyncSalesOfDelivery(QueryQuotationListReq request)
        {
            var result = new Response();
            try
            {
                await _app.SyncSalesOfDelivery(request);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request}, 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 取消销售订单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> CancellationSalesOrder(QueryQuotationListReq request)
        {
            var result = new Response();
            try
            {
                await _app.CancellationSalesOrder(request);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }
        /// <summary>
        /// 清空交货记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> EmptyDeliveryRecord(QueryQuotationListReq request) 
        {
            var result = new Response();
            try
            {
                await _app.EmptyDeliveryRecord(request);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request}, 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 申请取消销售订单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> CancelRequest(QueryQuotationListReq request) 
        {
            var result = new Response();
            await _app.CancelRequest(request);
            return result;
        }
        [HttpGet]
        /// <summary>
        /// 客户历史销售订单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> HistorySaleOrde([FromQuery]QueryQuotationListReq request)
        {
            return await _app.HistorySaleOrde(request);
        }

        [HttpGet]
        public async Task GenerateCommissionSettlement()
        {
            await _app.GenerateCommissionSettlement();
        }

        /// <summary>
        /// 我的提成
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> CommissionOrderList([FromQuery] QueryCommissionOrderReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.CommissionOrderList(request);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 待/已处理结算
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> CommissionReportList([FromQuery] QueryCommissionOrderReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.CommissionReportList(request);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 可提交的提成单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetAvailableCommissionOrder([FromQuery] QueryCommissionOrderReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.GetAvailableCommissionOrder(request);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 新建报表
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> AddReport(AddReportReq req)
        {
            var result = new Response();
            try
            {
                return await _app.AddReport(req);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 审核提成单
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> AccraditationCommissionOrder(AccraditationQuotationReq req)
        {
            var result = new Response();
            try
            {
                await _app.AccraditationCommissionOrder(req);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 批量结算
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> BatchAccraditation(BatchAccraditationReq req)
        {
            var result = new Response();
            try
            {
                await _app.BatchAccraditation(req);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 撤回报表
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> RecallReport(AccraditationQuotationReq req)
        {
            var result = new Response();
            try
            {
                await _app.RecallReport(req);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }

        [HttpGet]
        public async Task<TableData> DropDownOptions()
        {
            return await _app.DropDownOptions();
        }

        /// <summary>
        /// 审批详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetApproveDetail(int id)
        {
            var result = new TableData();
            try
            {
                return await _app.GetApproveDetail(id);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{id}, 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 查看附件
        /// </summary>
        /// <param name="saleOrderId"></param>
        /// <param name="type">1.销售合同 2.发票</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> ViewAttachment(int saleOrderId, int type)
        {
            var result = new TableData();
            try
            {
                return await _app.ViewAttachment(saleOrderId, type);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{saleOrderId}，{type}, 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 提成单详情 app用
        /// </summary>
        /// <param name="saleOrderId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> CommissionOrderDetail([FromQuery] QueryCommissionOrderReq req)
        {
            var result = new TableData();
            try
            {
                return await _app.CommissionOrderDetail(req);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.SalesOrderId}, 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 打印报表
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <param name="sign"></param>
        /// <param name="timespan"></param>
        /// <returns></returns>
        [HttpGet]
        //[ServiceFilter(typeof(CertAuthFilter))]
        public async Task<IActionResult> PrintCommissionReport(string serialNumber, string sign, string timespan)
        {
            try
            {
                return File(await _app.PrintCommissionReport(serialNumber), "application/pdf");
            }
            catch (Exception e)
            {
                Log.Logger.Error($"地址：{Request.Path}，参数：{serialNumber},{sign},{timespan}, 错误：{e.Message}");
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 销售收款
        /// </summary>
        /// <param name="saleOrderId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetOrderPaymentAndFee([FromQuery] QueryCommissionOrderReq req)
        {
            var result = new TableData();
            try
            {
                return await _app.GetOrderPaymentAndFee(req.Id);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.SalesOrderId}, 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetMaterialDetial(int Id)
        {
            var result = new TableData();
            try
            {
                List<string> list = new List<string>();
                list.Add("CT-4016-5V100A-NTA");
                await _app.GetMaterialDetial(Id, list);

            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
                //Log.Logger.Error($"地址：{Request.Path}，参数：{req.SalesOrderId}, 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 查看销售费用
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetCommissionOrderMoney(int commissionOrderId)
        {
            var result = new TableData();
            try
            {
                return await _app.GetCommissionOrderMoney(commissionOrderId);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{commissionOrderId}, 错误：{result.Message}");
            }
            return result;


        }
        /// <summary>
        /// 保存销售费用
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> AddOrUptCommissionOrderMoney(List<CommissionOrderMoney> req)
        {

            var result = new TableData();
            try
            {
                return await _app.AddOrUptCommissionOrderMoney(req);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req}, 错误：{result.Message}");
            }
            return result;
        }
    }
}
