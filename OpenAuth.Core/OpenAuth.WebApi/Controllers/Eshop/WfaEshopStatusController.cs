using System;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Serilog;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// wfa_eshop_status操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Eshop")]
    public class WfaEshopStatusController : Controller
    {
        private readonly WfaEshopStatusApp _wfastatusapp;

        public WfaEshopStatusController(WfaEshopStatusApp app)
        {
            _wfastatusapp = app;
        }

        /// <summary>
        /// 根据商城注册电话查对应订单状态列表
        /// </summary>
        /// <param name="MobileNo"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        //[AllowAnonymous]
        public async Task<TableData> GetOrderStatusByRegMobile([FromQuery] QryWfaEshopStatusListReq req)
        {
            var result = new TableData();
            try
            {
                result.Data = await _wfastatusapp.GetOrderStatusByRegMobile(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"接口：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 根据主键获取进度明细信息
        /// </summary>
        /// <param name="DocumentId"></param>
        /// <returns></returns>
        [HttpGet]
        //[AllowAnonymous]
        public async Task<Response<WfaEshopStatusResp>> GetStatusInfoById(int DocumentId)
        {
            var result = new Response<WfaEshopStatusResp>();
            try
            {
                result.Result = await _wfastatusapp.GetStatusInfoById(DocumentId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"接口：{Request.Path}，参数：{DocumentId}, 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        /// 找到业务伙伴当前对应业务员的手机号
        /// </summary>
        /// <param name="CardCode"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<string>> GetSalesPersonTelByCardCode(string CardCode)
        {
            var result = new Response<string>();
            try
            {
                result.Result = await _wfastatusapp.GetSalesPersonTelByCardCode(CardCode);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"接口：{Request.Path}，参数：{CardCode}, 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 通知物流已签收状态
        /// </summary>
        /// <param name="EshopPOrderNo"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> UpdateShipStatusForOrder(string EshopPOrderNo)
        {
            var result = new TableData();
            try
            {
                result = await _wfastatusapp.UpdateShipStatusForOrder(EshopPOrderNo);
    }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{EshopPOrderNo}， 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 保存商城订单传来的开票信息
        /// </summary>
        /// <param name="theReq">开票信息实体</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> UpdateEshopBillInfo(AddOrUpdatesale_ordr_pluginReq theReq)
        {
            var result = new TableData();
            try
            {
                result = await _wfastatusapp.UpdateEshopBillInfo(theReq);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{theReq.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }
    }
}
