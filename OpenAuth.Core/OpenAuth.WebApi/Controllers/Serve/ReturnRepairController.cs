using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Response;
using OpenAuth.App.Serve.Request;

namespace OpenAuth.WebApi.Controllers.Serve
{
    /// <summary>
    /// 返厂维修
    /// </summary>
    [Route("api/Serve/[controller]/[action]")]
    [ApiController]
    public class ReturnRepairController : ControllerBase
    {
        private readonly ReturnRepairApp _returnRepairApp;

        public ReturnRepairController(ReturnRepairApp returnRepairApp)
        {
            _returnRepairApp = returnRepairApp;
        }

        /// <summary>
        /// 提交物流信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> AddReturnRepair(AddReturnRepairReq request)
        {
            var result = new Response();
            try
            {
                await _returnRepairApp.AddReturnRepair(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 获取维修进度
        /// </summary>
        /// <param name="ServiceOrderId"></param>
        /// <param name="MaterialType"></param>
        /// <param name="AppUserId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetReturnRepairProcess(int ServiceOrderId, string MaterialType, int AppUserId)
        {
            var result = new TableData();
            try
            {
                result = await _returnRepairApp.GetReturnRepairProcess(ServiceOrderId, MaterialType, AppUserId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 获取返厂维修列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetReturnRepairList([FromQuery] QueryRerurnRepairListReq req)
        {
            var result = new TableData();
            try
            {
                result = await _returnRepairApp.GetReturnRepairList(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 修改快递单号
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> EditExpressNum(EditExpressNumReq req)
        {
            var result = new TableData();
            try
            {
                result = await _returnRepairApp.EditExpressNum(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 获取物流信息（返厂维修）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetExpressInfo(string Id)
        {
            var result = new TableData();
            try
            {
                result = await _returnRepairApp.GetExpressInfo(Id);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 寄出设备
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> AddExpress(AddExpressReq request)
        {
            var result = new Response();
            try
            {
                await _returnRepairApp.AddExpress(request);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 撤回快递
        /// </summary>
        /// <param name="ExpresssId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> WithDrawExpress(string ExpresssId)
        {
            var result = new Response();
            try
            {
                await _returnRepairApp.WithDrawExpress(ExpresssId);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }
    }
}
