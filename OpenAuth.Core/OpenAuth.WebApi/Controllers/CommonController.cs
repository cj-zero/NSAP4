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

        #region 主页报表
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
                if (request.CallType == 1)
                {
                    return await _app.ServiceCall(request);
                }
                else if (request.CallType == 2)
                {
                    return await _app.GetServiceCallInfo(request);
                }
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

        /// <summary>
        /// 根据生产部门获取服务单信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetServiceCallInfo([FromQuery] QueryReportReq req)
        {
            var result = new TableData();
            try
            {
                return await _app.GetServiceThemeInfo(req);
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
        /// 技术员接工单数
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetTechnicianOrderInfo([FromQuery] QueryReportReq req)
        {
            var result = new TableData();
            try
            {
                return await _app.GetTechnicianOrderInfo(req);
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
        /// 行程日报问题描述
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetProblemStatisticsInfo([FromQuery] QueryReportReq req)
        {
            var result = new TableData();
            try
            {
                return await _app.GetProblemStatisticsInfo(req);
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
        /// 行程日报解决方案
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetSolutionStatisticsInfo([FromQuery] QueryReportReq req)
        {
            var result = new TableData();
            try
            {
                return await _app.GetSolutionStatisticsInfo(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }
        #endregion

        #region 服务呼叫报表
        /// <summary>
        /// 服务呼叫分布
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> ServiceOrderDistribution([FromQuery] QueryReportReq req)
        {
            return await _app.ServiceOrderDistribution(req);
        }

        /// <summary>
        /// 服务呼叫来源
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> ServiceOrderSource([FromQuery] QueryReportReq req)
        {
            return await _app.ServiceOrderSource(req);
        }

        /// <summary>
        /// 服务呼叫状态和问题类型分析
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> ServiceOrderStatusAndProblemType([FromQuery] QueryReportReq req)
        {
            return await _app.ServiceOrderStatusAndProblemType(req);
        }

        /// <summary>
        /// 催办次数
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> UrgingTimes([FromQuery] QueryReportReq req)
        {
            return await _app.UrgingTimes(req);
        }
        /// <summary>
        /// 呼叫主题进度分析
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> ServiceThemeProgress([FromQuery] QueryReportReq req)
        {
            return await _app.ServiceThemeProgress(req);
        }
        #endregion

        #region 报销模块报表
        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> CostAnalysisReport([FromQuery] QueryReimburseInfoListReq req)
        {
            var result = new TableData();
            try
            {
                return await _app.CostAnalysisReport(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }
        #endregion


        /// <summary>
        /// 主管查看费用归属报表-结算
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> AnalysisReportCostManager([FromQuery] QueryoutsourcListReq req)
        {
            var result = new TableData();
            try
            {
                return await _app.AnalysisReportCostManager(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }
    }
}
