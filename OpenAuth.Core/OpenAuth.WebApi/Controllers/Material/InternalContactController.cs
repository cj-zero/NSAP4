using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Material;
using OpenAuth.App.Material.Request;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.WebApi.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Material
{
    /// <summary>
    /// 内部联络单
    /// </summary>
    [Route("api/Material/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName ="Material")]
    public class InternalContactController : ControllerBase
    {
        private InternalContactApp _app;
        public InternalContactController(InternalContactApp internalContactApp)
        {
            _app = internalContactApp;
        }

        /// <summary>
        /// 获取联络单列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> Load([FromQuery] QueryInternalContactReq req)
        {
            return await _app.Load(req);
        }

        /// <summary>
        /// 添加联络单
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Add(AddOrUpdateInternalContactReq obj)
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetDetails(int id)
        {
            return await _app.GetDetails(id);
        }

        /// <summary>
        /// 查看工单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> WorkOrder(int id)
        {
            return await _app.WorkOrder(id);
        }

        /// <summary>
        /// 审批、执行
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Accraditation(AccraditationInternalContactReq req)
        {
            Response result = new Response();
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
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response> SebdEmail(int id)
        {
            Response result = new Response();
            try
            {
                await _app.SendEmail(id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：id:{id}, 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 撤销\过期\停用\启用
        /// </summary>
        /// <param name="internalContactId"></param>
        /// <param name="isRevoke"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response> Revocation(int internalContactId, int handleType)
        {
            Response result = new Response();
            try
            {
                await _app.Revocation(internalContactId, handleType);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{internalContactId},{handleType}, 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <param name="sign"></param>
        /// <param name="timespan"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(CertAuthFilter))]
        [HttpGet]
        public async Task<IActionResult> PrintInternalContact(string serialNumber, string sign, string timespan)
        {
            try
            {
                return File(await _app.PrintInternalContact(serialNumber), "application/pdf");
            }
            catch (Exception e)
            {
                Log.Logger.Error($"地址：{Request.Path}，参数：{serialNumber},{sign},{timespan}, 错误：{e.Message}");
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        [HttpPost]
        public TableData UploadImg(IFormFileCollection files)
        {
            TableData data = new TableData();
            data.Data = _app.UpdloadImg(files);
            return data;
        }

        /// <summary>
        /// 获取生产订单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        //[HttpPost]
        //public async Task<TableData> GetProductionOrder(QueryProductionOrderReq req)
        //{
        //    return await _app.GetProductionOrder(req);
        //}

        ///// <summary>
        ///// 获取未出货物料，归属部门
        ///// </summary>
        ///// <param name="req"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public async Task<TableData> GetUndeliveredMaterial(QueryProductionOrderReq req)
        //{
        //    return await _app.GetUndeliveredMaterial(req);
        //}

        ///// <summary>
        ///// 获取已出货物料，设备序列号
        ///// </summary>
        ///// <param name="req"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public async Task<TableData> GetShippedMaterial(QueryProductionOrderReq req)
        //{
        //    return await _app.GetShippedMaterial(req);
        //}

        //



        /// <summary>
        /// 获取客户信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> CustomerInfo([FromQuery] QueryCustomerOrder req)
        {
            return await _app.CustomerInfo(req);
        }

        /// <summary>
        /// 获取销售订单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> SaleOrderInfo([FromQuery] QueryCustomerOrder req)
        {
            return await _app.SaleOrderInfo(req);
        }

        /// <summary>
        /// 获取成品物料前缀
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetItemType()
        {
            TableData result = new TableData();
            result=await _app.GetItemType();
            return result;
        }

        /// <summary>
        /// 获取特殊要求、系列下拉框
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetItemTypeExtra()
        {
            TableData result = new TableData();
            result = await _app.GetItemTypeExtra();
            return result;
        }

        /// <summary>
        /// 获取设备生产情况（生产单）
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> ProductionOrderInfo(List<QueryProductionOrderReq> req)
        {
            return await _app.ProductionOrderInfo(req);
        }
        /// <summary>
        /// 生成服务单、任务单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task GenerateWorkOrder(int id)
        {
            await _app.GenerateWorkOrder(id);
        }
        /// <summary>
        /// 获取内联单内容
        /// </summary>
        /// <param name="serviceOrderId"></param>
        /// <param name="vestInOrg"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> GetInternalContactContent(int serviceOrderId, int vestInOrg)
        {
            return await _app.GetInternalContactContent(serviceOrderId, vestInOrg);
        }

        [HttpGet]
        public async Task<Response> AsyncData()
        {
            Response result = new Response();
            try
            {
                await _app.AsyncData();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}, 错误：{result.Message}");
            }
            return result;
        }
    }
}
