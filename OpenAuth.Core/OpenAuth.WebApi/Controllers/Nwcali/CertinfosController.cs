using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using Serilog;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 校准证书信息操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Nwcali")]
    public class CertinfosController : ControllerBase
    {
        private readonly CertinfoApp _app;

        /// <summary>
        /// 加载列表
        /// </summary>
        [HttpGet]
        public async Task<TableData> Load([FromQuery]QueryCertinfoListReq request)
        {
            return await _app.LoadAsync(request);
        }

        /// <summary>
        /// 证书待审批流程查询列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> LoadApprover([FromQuery]QueryCertApproverListReq req)
        {
            return await _app.LoadApprover(req);
        }

        /// <summary>
        /// 业务员证书查询列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> LoadSaleManLoad([FromQuery] QuerySaleOrDeviceOrCertListReq req)
        {
            var result = new TableData();
            try
            {
                result=await _app.LoadSaleInfo(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message= ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{ex.Message}");
            }
            return result;
        }

        /// <summary>
        /// 证书审批操作
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> CertVerification(List<CertVerificationReq> req)
        {
            var result = new Response();
            try
            {
                result = await _app.CertVerification(req);
            }
            catch(Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{ex.Message}");
            }
            return result;
        }

        /// <summary>
        /// 查询委托单列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> LoadEntrustment([FromQuery] QueryEntrustmentReq req)
        {
            var result = new TableData();
            try
            {
                result = await _app.LoadEntrustment(req);
            
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{ex.Message}");
            }
            return result;
        }

        /// <summary>
        /// 查询委托单详情
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetDetail(int Id)
        {
            var result = new TableData();
            try
            {
                result = await _app.GetDetail(Id);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{Id}， 错误：{ex.Message}");
            }
            return result;
        }

        /// <summary>
        /// 处理委托单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response> HandleEntrusted([FromQuery] HandleEntrustedReq req)
        {
            var result = new Response();
            try
            {
                result = await _app.HandleEntrusted(req);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{ex.Message}");
            }
            return result;
        }

        [HttpGet]
        public async Task<Response> SynSalesDelivery()
        {
            var result = new Response();
            try
            {
                await _app.SynSalesDelivery();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message + "，堆栈信息：" + ex.StackTrace;
            }
            return result;
        }

        public CertinfosController(CertinfoApp app) 
        {
            _app = app;
        }
    }
}
