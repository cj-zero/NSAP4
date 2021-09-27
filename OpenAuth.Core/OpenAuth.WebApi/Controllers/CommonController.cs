using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 公共操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "General")]
    public class CommonController : ControllerBase
    {
        private CommonApp _app;
        public CommonController(CommonApp app)
        {
            _app = app;
        }

        /// <summary>
        /// 主页顶部数量汇总
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> HomePageCardInfo()
        {
            return await _app.GetTopNumCard();
        }

        /// <summary>
        /// 首页图表统计
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> HomePageChartInfo()
        {
            return await _app.GetChartInfo();
        }

        /// <summary>
        /// 呼叫来源
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> CallSource([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.CallSource(request);
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
        /// 服务呼叫
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> ServiceCall([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.ServiceCall(request);
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
        /// 报销金额与客诉服务
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> Reimburseinfo([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.Reimburseinfo(request);
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
        /// 销售金额
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> SaleAmount([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.SaleAmount(request);
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
        /// 个代金额
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> SettlementAmount([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.SettlementAmount(request);
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
        /// 获取客诉服务
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetServiceOrderInfo([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.GetServiceOrderInfo(request);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }
    }
}
