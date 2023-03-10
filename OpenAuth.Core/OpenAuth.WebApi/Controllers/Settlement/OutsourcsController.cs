using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Settlement.Request;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Settlement;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// outsourc操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Settlement")]
    public class OutsourcsController : ControllerBase
    {
        private readonly OutsourcApp _app;

        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetDetails([FromQuery] QueryoutsourcListReq request)
        {
            return await _app.GetDetails(request);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Add(AddOrUpdateoutsourcReq obj)
        {
            var result = new Response();
            await _app.Add(obj);
            return result;
        }

        /// <summary>
        /// 审批
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Accraditation(AccraditationOutsourcReq req)
        {
            var result = new Response();
            await _app.Accraditation(req);
            return result;
        }
        /// <summary>
        /// 批量审批
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> BatchAccraditation(List<AccraditationOutsourcReq> req)
        {
            var result = new Response();
            await _app.BatchAccraditation(req);
            return result;
        }
        /// <summary>
        /// 驳回单个
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> ASingleRejection(AccraditationOutsourcReq req)
        {
            return await _app.ASingleRejection(req);
        }

        /// <summary>
        /// 撤回单个
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> SingleRecall(AccraditationOutsourcReq req)
        {
            var result = new Response();
            try
            {
                await _app.SingleRecall(req);
            }
            catch (Exception ex)
            {
                result.Message = ex?.Message ?? ex?.InnerException?.Message ?? "";
                result.Code = 500;
            }

            return result;
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Update(AddOrUpdateoutsourcReq obj)
        {
            var result = new Response();
            try
            {
                await _app.Update(obj);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 加载列表
        /// </summary>
        [HttpGet]
        public async Task<TableData> Load([FromQuery] QueryoutsourcListReq request)
        {
            return await _app.Load(request);
        }
        /// <summary>
        /// 导出待支付Excel
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Export([FromQuery] QueryoutsourcListReq req)
        {
            var data = await _app.Export(req);


            return File(data, "application/vnd.ms-excel");
        }

        /// <summary>
        /// 结算费用归属
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> LoadCostAttribution([FromQuery] QueryoutsourcListReq request)
        {
            return await _app.LoadCostAttribution(request);
        }

        /// <summary>
        /// 获取费用归属总金额
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetMoney([FromQuery] QueryoutsourcListReq request)
        {
            return await _app.GetMoney(request);
        }
        /// <summary>
        /// 主管查看费用归属报表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> AnalysisReportCostManager([FromQuery] QueryoutsourcListReq request)
        {
            return await _app.AnalysisReportCostManager(request);
        }
        /// <summary>
        /// 获取可结算服务单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetServiceOrder([FromQuery] QueryoutsourcListReq request)
        {
            return await _app.GetServiceOrder(request);
        }
        /// <summary>
        /// 查询服务单详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> ServiceOrderDetails(QueryoutsourcListReq request)
        {
            return await _app.ServiceOrderDetails(request);
        }
        /// <summary>
        /// 批量删除
        /// </summary>
        [HttpPost]
        public async Task<Response> Delete(QueryoutsourcListReq req)
        {
            var result = new Response();
            await _app.Delete(req);
            return result;
        }

        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ExportExcel([FromQuery] QueryoutsourcListReq req)
        {
            var data = await _app.ExportExcel(req);
            return File(data, "application/vnd.ms-excel");
        }


        /// <summary>
        /// 提交审核报表
        /// </summary>
        [HttpPost]
        public async Task<Response> CreateReport(CreateReportReq req)
        {
            return await _app.CreateReport(req);
        }

        /// <summary>
        /// 撤回报表
        /// </summary>
        [HttpPost]
        public async Task<Response> RevocationReport(QueryoutsourcListReq req)
        {
            return await _app.RevocationReport(req.Id);
        }

        /// <summary>
        /// 解散报表
        /// </summary>
        [HttpPost]
        public async Task<Response> DissolutionReport(QueryoutsourcListReq req)
        {
            return await _app.DissolutionReport(req.Id);
        }

        /// <summary>
        /// 获取报表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetReport([FromQuery] QueryoutsourcListReq req)
        {
            return await _app.GetReport(req);
        }

        /// <summary>
        /// 获取报表详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetReportDetail([FromQuery] QueryoutsourcListReq request)
        {
            return await _app.GetReportDetail(request);
        }

        /// <summary>
        /// 通过报表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task BatchAccraditationByReport(QueryoutsourcListReq req)
        {
            await _app.BatchAccraditationByReport(req.Id);
        }

        //[HttpGet]
        //public async Task Test()
        //{
        //    await _app.Test();
        //}

        /// <summary>
        /// 定时刷新数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task UpdateDataJob()
        {
            await _app.UpdateDataJob();
        }

        public OutsourcsController(OutsourcApp app)
        {
            _app = app;
        }
    }
}
