using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Nwcali.Request;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.WebApi.Model;
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
        /// 批量删除证书
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Infrastructure.Response> DeleteCertinfo(List<string> ids)
        {
            var result = new Response();
            try
            {
                result = await _app.DeleteCertinfo(ids);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ids.ToJson()}， 错误：{ex.Message}");
            }
            return result;
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
        /// 批量生成证书文件
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> BatchCreateNwcailFile(List<string> certNo)
        {
            var result = new Response();
            try
            {
                result=await _app.BatchCreateNwcailFile(certNo);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{certNo.ToJson()}， 错误：{ex.Message}");
            }
            return result;
        }

        /// <summary>
        /// 未生成证书 生成证书
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task CreateNwcailFileHelper()
        {
            await _app.CreateNwcailFileHelper();
        }

        /// <summary>
        /// 重新生成证书数据
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task ReFillNwcailData(string certNo)
        {
            var result = new Response();
            try
            {
                await _app.ReFillNwcailData(certNo);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{certNo.ToJson()}， 错误：{ex.Message}");
            }
            //return result;
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

        /// <summary>
        /// 查询生产订单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> ProductionOrderList([FromQuery] QueryCertinfoListReq request)
        {
            return await _app.ProductionOrderList(request);
        }

        /// <summary>
        /// 生产单物料详情
        /// </summary>
        /// <param name="docEntry"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> MaterialDetail(int docEntry)
        {
            return await _app.MaterialDetail(docEntry);
        }
        /// <summary>
        /// 生成生产唯一码
        /// </summary>
        /// <param name="docEntry"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response> GenerateWO()
        {
            Response result = new Response();
            try
            {
                await _app.GenerateWO();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 生产单基本信息
        /// </summary>
        /// <param name="docEntry"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetOworDetail(int docEntry)
        {
            return await _app.GetOworDetail(docEntry);
        }

        /// <summary>
        /// 生产进度
        /// </summary>
        /// <param name="docEntry"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetScheduleInfo([FromQuery] QueryProductionScheduleReq req)
        {
            return await _app.GetScheduleInfo(req);
        }

        /// <summary>
        /// 获取通道结果
        /// </summary>
        /// <param name="docEntry"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetChlIdResult(string guid)
        {
            return await _app.GetChlIdResult(guid);
        }

        /// <summary>
        /// 获取下位机
        /// </summary>
        /// <param name="docEntry"></param>
        /// <returns></returns>
        [HttpGet]
        public Response<List<string>> GetLowGuid(string wo)
        {
            Response<List<string>> result = new Response<List<string>>();
            result.Result= _app.GetLowGuid(wo);
            return result;
        }
        ///// <summary>
        ///// 生产唯一码下设备是否校准
        ///// </summary>
        ///// <param name="wo"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public async Task<TableData> CheckCalibration(string wo)
        //{
        //    return await _app.CheckCalibration(wo);
        //}

        /// <summary>
        /// 收货
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(RequestActionFilter))]
        public async Task<Response> ProductionReceipt(List<ReceiveReq> list)
        {
            Response result = new Response();
            try
            {
                await _app.ProductionReceipt(list);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
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
