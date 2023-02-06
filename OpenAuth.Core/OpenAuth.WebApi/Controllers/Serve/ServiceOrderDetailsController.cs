using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Response;
using OpenAuth.App.Serve;
using Serilog;
using System;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Serve
{
    /// <summary>
    /// 服务单详情
    /// </summary>
    [Route("api/serve/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Serve")]
    public class ServiceOrderDetailsController : Controller
    {

        private readonly ServiceOrderDetailsApp _serviceOrderDetailsApp;

        public ServiceOrderDetailsController(ServiceOrderDetailsApp serviceOrderDetailsApp)
        {
            _serviceOrderDetailsApp = serviceOrderDetailsApp;
        }

        /// <summary>
        /// 获取按灯记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public Task<TableData> GetBlameBelongInfo([FromQuery] int serviceOrderId)
        {
            var result = new TableData();
            try
            {
                result.Data = _serviceOrderDetailsApp.GetBlameBelongInfo(serviceOrderId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{serviceOrderId}， 错误：{result.Message}");
            }
            return Task.FromResult(result);
        }
        /// <summary>
        /// 获取工单数据
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public Task<TableData> ServiceWorkOrderListDetail([FromQuery] int serviceOrderId)
        {
            var result = new TableData();
            try
            {
                result.Data = _serviceOrderDetailsApp.ServiceWorkOrderListDetail(serviceOrderId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{serviceOrderId}， 错误：{result.Message}");
            }
            return Task.FromResult(result);
        }
     
        /// <summary>
        /// 获取技术员数据
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public Task<TableData> GetTechnician([FromQuery] int serviceOrderId)
        {
            var result = new TableData();
            try
            {
                result.Data = _serviceOrderDetailsApp.GetTechnician(serviceOrderId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{serviceOrderId}， 错误：{result.Message}");
            }
            return Task.FromResult(result);
        }
 
        /// <summary>
        /// 获取报销记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public Task<TableData> GetReimbursement([FromQuery] int serviceOrderId)
        {
            var result = new TableData();
            try
            {
                result.Data = _serviceOrderDetailsApp.GetReimbursement(serviceOrderId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{serviceOrderId}， 错误：{result.Message}");
            }
            return Task.FromResult(result);
        }
        /// <summary>
        /// 获取提成记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public Task<TableData> GetCommission([FromQuery] int serviceOrderId)
        {
            var result = new TableData();
            try
            {
                result.Data = _serviceOrderDetailsApp.GetCommission(serviceOrderId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{serviceOrderId}， 错误：{result.Message}");
            }
            return Task.FromResult(result);
        }
        /// <summary>
        /// 获取历史记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public Task<TableData> GetHistory([FromQuery] int serviceOrderId)
        {
            var result = new TableData();
            try
            {
                result.Data = _serviceOrderDetailsApp.GetHistory(serviceOrderId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{serviceOrderId}， 错误：{result.Message}");
            }
            return Task.FromResult(result);
        }
    }
}
