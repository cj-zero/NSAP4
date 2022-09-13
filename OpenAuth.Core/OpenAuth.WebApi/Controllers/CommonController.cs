using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
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
        /// 部门工单数量
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> ServiceCallOrg([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.ServiceCallOrg(request);
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
        /// 报销金额详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> ReimburseinfoDetail([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.ReimburseinfoDetail(request);
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
        /// 部门或个人报销信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> DeptOrUserOfReimburseinfo([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.DeptOrUserOfReimburseinfo(request);
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
        /// 销售金额详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> SaleAmountDetail([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.SaleAmountDetail(request);
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
        /// 提成金额与提成单数量
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> CommissionAmount([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.CommissionAmount(request);
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
        /// 提成报表详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> CommissionDetail([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.CommissionDetail(request);
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
        /// 部门或个人提成信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> DeptOrUserOfCommission([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.DeptOrUserOfCommission(request);
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
        /// 个代金额详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> SettlementAmountDetail([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.SettlementAmountDetail(request);
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
        /// 部门或个人个代金额
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> DeptOrUserOfSettlementAmount([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.DeptOrUserOfSettlementAmount(request);
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
        /// 报销金额归属（按部门）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> ReimburseinfoAscription([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.ReimburseinfoAscription(request);
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
        /// 报销金额归属与解决方案
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> ReimburseinfoSolution([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.ReimburseinfoSolution(request);
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
        /// 个代金额归属（按部门）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> SettlementAscription([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.SettlementAscription(request);
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
        /// 个代金额归属与解决方案
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> SettlementSolution([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.SettlementSolution(request);
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
        /// 研发部门报销归属及问题描述
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> DevelopCostAttribution([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.DevelopCostAttribution(request);
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
        /// 研发部门报销归属及问题描述--详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> DevelopCostAttributionDeteail([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.DevelopCostAttributionDeteail(request);
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
        /// 业务员名下客户服务呼叫
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> SaleManServiceOrderReport([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.SaleManServiceOrderReport(request);
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
        /// 业务员名下客户服务呼叫详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> SaleManServiceOrderReportDetail([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.SaleManServiceOrderReportDetail(request);
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
        /// 业务员名下呼叫工单数与呼叫主题
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> SaleManServiceOrderFromTheme([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.SaleManServiceOrderFromTheme(request);
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
        /// 业务员名下呼叫工单数与解决方案
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> SaleManServiceOrderSolution([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.SaleManServiceOrderSolution(request);
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
        /// 业务主管部门下业务员工单数量和呼叫主题
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> SaleManDeptServiceOrderFromTheme([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.SaleManDeptServiceOrderFromTheme(request);
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
        /// 业务主管部门下业务员工单数量和解决方案
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> SaleManDeptServiceOrderSolution([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.SaleManDeptServiceOrderSolution(request);
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
        /// 工单进行中
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetUnFinishServiceCallProcessingTime([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.GetUnFinishServiceCallProcessingTime(request);
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
        /// 工单已完成
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetFinishServiceCallProcessingTime([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.GetFinishServiceCallProcessingTime(request);
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
        /// 工单数量与处理时效
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetServerCallEfficiency([FromQuery] QueryReportReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.GetServerCallEfficiency(request);
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
        /// 生产部门与工单数量
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> ProductionOrgServiceOrder([FromQuery] QueryReportReq req)
        {
            var result = new TableData();
            try
            {
                return await _app.ProductionOrgServiceOrder(req);
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
        /// 生产部门呼叫主题数量统计
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> ProductionOrgFromTheme([FromQuery] QueryReportReq req)
        {
            var result = new TableData();
            try
            {
                return await _app.ProductionOrgFromTheme(req);
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
        /// 催办次数详情
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> UrgingTimesDetail([FromQuery] QueryReportReq req)
        {
            return await _app.UrgingTimesDetail(req);
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

        #region 报表配置
        /// <summary>
        /// 获取报表配置列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetReportInfo([FromQuery] QueryReportReq req)
        {
            var result = new TableData();
            try
            {
                return await _app.GetReportInfo(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}， 错误：{result.Message}");
            }
            return result;

        }
        /// <summary>
        /// 修改报表配置
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> UpdateReport(ReportInfo req)
        {
            var result = new Response();
            try
            {
                await _app.UpdateReport(req);
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
        /// 添加报表配置
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> AddReport(ReportInfo req)
        {
            var result = new Response();
            try
            {
                await _app.AddReport(req);
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
        /// 分配报表角色
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public Response AssignRoleReport(AssignReq request)
        {
            var result = new Response();
            try
            {
                _app.AssignRoleReport(request);
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
        /// 获取角色已分配的报表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetRoleReport(string id)
        {
            var result = new TableData();
            try
            {
                return await _app.GetRoleReport(id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{id}, 错误：{result.Message}");
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
