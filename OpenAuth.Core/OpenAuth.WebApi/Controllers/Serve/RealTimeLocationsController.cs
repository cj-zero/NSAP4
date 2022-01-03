using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Serve;
using OpenAuth.Repository.Domain;
using Serilog;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 定位相关操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Serve")]
    public class RealTimeLocationsController : ControllerBase
    {
        private readonly RealTimeLocationApp _realTimeLocationApp;
        private readonly RealTimeLocationPush _realTimeLocationPush;
        public RealTimeLocationsController(RealTimeLocationApp realTimeLocationApp, RealTimeLocationPush realTimeLocationPush)
        {
            _realTimeLocationApp = realTimeLocationApp;
            _realTimeLocationPush = realTimeLocationPush;
        }

        /// <summary>
        /// 获取定位信息
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="ServiceOrderId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> Load(int ServiceOrderId, string UserId)
        {
            var result = new TableData();
            try
            {
                result = await _realTimeLocationApp.Load(ServiceOrderId, UserId);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ServiceOrderId},{UserId}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 添加定位信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Add(AddOrUpdaterealtimelocationReq req)
        {
            var result = new Response();
            try
            {
                await _realTimeLocationApp.Add(req);

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
        /// 智慧大屏接口数据
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> LoadLocationInfo([FromBody] QueryLocationInfoReq req)
        {
            var result = new TableData();
            result = await _realTimeLocationApp.LoadLocationInfo(req);
            return result;
        }

        /// <summary>
        /// 获取未完工状态的客诉单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> LoadServiceOrder()
        {
            var result = new TableData();
            result = await _realTimeLocationApp.LoadServiceOrder();
            return result;
        }

        /// <summary>
        /// 查询技术员轨迹
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> LoadTrajectory([FromQuery] QueryTrajectoryReq req)
        {
            var result = new TableData();
            result = await _realTimeLocationApp.HistoryTrajectory(req);
            return result;
        }

        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ExportExcel([FromBody] QueryLocationInfoReq req)
        {
            var data =await _realTimeLocationApp.ExcelAttendanceInfo(req);
            return File(data, "application/vnd.ms-excel");
        }

        /// <summary>
        /// 分析报表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> OnlineDurationReport([FromBody] QueryLocationInfoReq req)
        {
            var result = new TableData();
            result = await _realTimeLocationApp.OnlineDurationReport(req);
            return result;
        }

        /// <summary>
        /// 获取查看人员名单
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetLoactionViewUser()
        {
            return await _realTimeLocationApp.GetLoactionViewUser();
        }


        /// <summary>
        /// 保存查看人员名单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> SetLoactionViewUser(QueryLocationInfoReq req)
        {
            await _realTimeLocationApp.SetLoactionViewUser(req);
            return new Response();
        }

        /// <summary>
        ///获取所有客户
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetCustomer()
        {
            var res = new TableData();
            res = await _realTimeLocationApp.GetCustomer();
            return res;
        }

    }
}
