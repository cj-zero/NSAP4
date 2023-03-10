using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Serve.Request;
using OpenAuth.App.Serve.Response;
using OpenAuth.WebApi.Model;
using Serilog;

namespace OpenAuth.WebApi.Controllers.Serve
{
    /// <summary>
    /// 报销
    /// </summary>
    [Route("api/serve/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Serve")]
    public class ReimburseController : ControllerBase
    {
        private readonly ReimburseInfoApp _reimburseinfoapp;

        private readonly CategoryApp _categoryapp;

        public ReimburseController(ReimburseInfoApp reimburseinfoapp, CategoryApp categoryapp)
        {
            _reimburseinfoapp = reimburseinfoapp;
            _categoryapp = categoryapp;
        }
        /// <summary>
        /// 查看报销单列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> Load([FromQuery] QueryReimburseInfoListReq request)
        {
            var result = new TableData();
            try
            {
                return await _reimburseinfoapp.Load(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        /// 获取总金额待支付已支付金额
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetMoney([FromQuery] QueryReimburseInfoListReq request)
        {
            return await _reimburseinfoapp.GetMoney(request);
        }
        /// <summary>
        /// 获取费用归属总金额
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetMoneyForCost([FromQuery] QueryReimburseInfoListReq request)
        {
            return await _reimburseinfoapp.GetMoneyForCost(request);
        }
        /// <summary>
        /// App查看报销单列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> AppLoad([FromQuery] QueryReimburseInfoListReq request)
        {
            var result = new TableData();
            try
            {
                return await _reimburseinfoapp.AppLoad(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 获取未报销服务单列别
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetServiceOrder([FromQuery] QueryReimburseServerOrderListReq request)
        {
            var result = new TableData();
            try
            {
                return await _reimburseinfoapp.GetServiceOrder(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetUserDetails([FromQuery] QueryReimburseServerOrderListReq request)
        {
            var result = new TableData();
            try
            {
                return await _reimburseinfoapp.GetUserDetails(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{request.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        /// 获取报销单
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetDetails(int ReimburseInfoId)
        {
            var result = new TableData();
            try
            {
                return await _reimburseinfoapp.GetDetails(ReimburseInfoId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ReimburseInfoId}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 获取字典
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetListCategoryName()
        {
            var result = new TableData();
            string ids = "SYS_ReimburseAccraditation,SYS_ReimburseAccommodation,SYS_OtherExpenses,SYS_Transportation,SYS_TransportationAllowance,SYS_TravellingAllowance,SYS_ServiceRelations,SYS_RemburseStatus,SYS_ReimburseType,SYS_Responsibility,SYS_ProjectName,SYS_Expense";
            try
            {
                return await _categoryapp.GetListCategoryName(ids);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 添加报销单
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public Response Add(AddOrUpdateReimburseInfoReq obj)
        {
            var result = new Response();
            try
            {
                _reimburseinfoapp.Add(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        /// 新增出差补贴
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> AddTravellingAllowance(ReimburseTravellingAllowanceResp obj)
        {
            var result = new TableData();
            try
            {
                return await _reimburseinfoapp.AddTravellingAllowance(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        /// 修改报销单
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public Response UpDate(AddOrUpdateReimburseInfoReq obj)
        {
            var result = new Response();
            try
            {
                _reimburseinfoapp.UpDate(obj);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        /// 删除报销费用
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> DeleteCost(ReimburseRevocationReq req)
        {
            var result = new Response();
            try
            {
                _reimburseinfoapp.DeleteCost(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 撤回操作
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> Revocation(ReimburseRevocationReq req)
        {
            var result = new TableData();
            try
            {
                return await _reimburseinfoapp.Revocation(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 审批
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> Accraditation(AccraditationReimburseInfoReq req)
        {
            var result = new TableData();
            try
            {
                await _reimburseinfoapp.Accraditation(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 设置费用部门
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> SetExpenseOrgs(AccraditationReimburseInfoReq req)
        {
            var result = new TableData();
            try
            {
                await _reimburseinfoapp.SetExpenseOrgs(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 批量审批
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> BatchAccraditation(AccraditationReimburseInfoReq req)
        {
            var result = new TableData();
            try
            {
                await _reimburseinfoapp.BatchAccraditation(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 发票号码是否唯一
        /// </summary>
        /// <param name="InvoiceNumber"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> IsSole(List<string> InvoiceNumber)
        {
            var result = new Response();
            try
            {
                if (!await _reimburseinfoapp.IsSole(InvoiceNumber))
                {
                    throw new CommonException("添加报销单失败。发票存在已使用，不可二次使用！", Define.INVALID_InvoiceNumber);
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{InvoiceNumber.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 打印报销单
        /// </summary>
        /// <param name="ReimburseInfoId"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(CertAuthFilter))]
        [HttpGet]
        public async Task<FileResult> Print(string serialNumber, string sign, string timespan)
        {
            try
            {
                return File(await _reimburseinfoapp.Print(serialNumber), "application/pdf");
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"地址：{Request.Path}，参数：{serialNumber},{sign},{timespan}， 错误：{ex.Message}");
                throw new Exception("导出失败！" + ex.ToString());
            }
        }

        /// <summary>
        /// 删除报销单
        /// </summary>
        /// <param name="ReimburseInfoId"></param>
        /// <param name="AppId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Delete(ReimburseRevocationReq req)
        {
            var result = new Response();
            try
            {
                await _reimburseinfoapp.Delete(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        /// 客户历史报销单 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> HistoryReimburseInfo([FromQuery] QueryReimburseInfoListReq req)
        {
            var result = new TableData();
            try
            {
                return await _reimburseinfoapp.HistoryReimburseInfo(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 个人历史费用 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> HistoryReimburseInfoForUser([FromQuery] QueryReimburseInfoListReq req)
        {
            var result = new TableData();
            try
            {
                return await _reimburseinfoapp.HistoryReimburseInfoForUser(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        ///  个人历史费用-报销 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> HistoryDetailForUser([FromQuery] QueryHistoryDetailForUserReq req)
        {
            var result = new TableData();
            try
            {
                return await _reimburseinfoapp.HistoryDetailForUser(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        ///  个人历史费用-报销 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> HistoryForUserByReimburse([FromQuery] QueryReimburseInfoListReq req)
        {
            var result = new TableData();
            try
            {
                return await _reimburseinfoapp.HistoryForUserByReimburse(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }
        /// <summary>
        /// 个人历史费用-结算/代理 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> HistoryForUserByOutsourc([FromQuery] QueryReimburseInfoListReq req)
        {
            var result = new TableData();
            try
            {
                return await _reimburseinfoapp.HistoryForUserByOutsourc(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 个人历史费用-提成
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> HistoryForUserByCommission([FromQuery] QueryReimburseInfoListReq req)
        {
            var result = new TableData();
            try
            {
                return await _reimburseinfoapp.HistoryForUserByCommission(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }
            return result;
        }



        /// <summary>
        /// 根据指定时间获取费用单 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetExpenseBillByDate([FromQuery] QueryReimburseInfoListReq req)
        {
            return await _reimburseinfoapp.GetExpenseBillByDate(req);
        }

        /// <summary>
        /// 获取指定时间的费用 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetExpendsByDate([FromQuery] QueryReimburseInfoListReq req)
        {
            return await _reimburseinfoapp.GetExpendsByDate(req);
        }

        /// <summary>
        /// 获取服务轨迹起始点 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetTdianravelSpot([FromQuery] QueryReimburseInfoListReq req)
        {
            var result = new TableData();
            try
            {
                return await _reimburseinfoapp.GetTdianravelSpot(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 报表分析
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> AnalysisReport()
        {
            var result = new TableData();
            try
            {
                return await _reimburseinfoapp.AnalysisReport();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 导出待支付Excel
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Export([FromQuery] QueryReimburseInfoListReq req)
        {
            var data = await _reimburseinfoapp.Export(req);


            return File(data, "application/vnd.ms-excel");
        }

        /// <summary>
        /// 导出我的提交Excel
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ExportLoad([FromQuery] QueryReimburseInfoListReq req)
        {
            var data = await _reimburseinfoapp.ExportLoad(req);

            return File(data, "application/vnd.ms-excel");
        }

    }

}
