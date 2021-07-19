
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Order;
using OpenAuth.App.Order.Request;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Order
{
    /// <summary>
    /// 销售报价单
    /// </summary>
    [Route("api/Order/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Order")]
    public class OrderDraftController : Controller
    {
        private readonly ServiceSaleOrderApp _serviceSaleOrderApp;
        public OrderDraftController(ServiceSaleOrderApp serviceSaleOrderApp)
        {
            _serviceSaleOrderApp = serviceSaleOrderApp;
        }
        /// <summary>
        /// 获取业务员
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("SalesManInfo")]
        public async Task<Response<List<SelectOption>>> GetSalesManInfo()
        {
            var result = new Response<List<SelectOption>>();
            try
            {
                result.Result = _serviceSaleOrderApp.GetSalesSelect(0);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}, 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 销售报价单保存
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Save")]
        public async Task<Response<string>> Save(AddOrUpdateOrderReq orderReq)
        {
            var result = new Response<string>();
            try
            {
                result.Result = _serviceSaleOrderApp.Save(orderReq);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}, 错误：{result.Message}");
            }
            return result;
        }
    }
}
