using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.WMS.Request;
using OpenAuth.App.WMS;
using Infrastructure;
using Serilog;
using OpenAuth.App.Response;

namespace OpenAuth.WebApi.Controllers.WMS
{
    [Route("api/WMS/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "WMS")]
    public class ProductReceiptController : ControllerBase
    {
        private readonly ProductReceiptApp _productReceiptApp;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="app"></param>
        public ProductReceiptController(ProductReceiptApp app)
        {
            _productReceiptApp = app;
        }
        /// <summary>
        /// 同步生产收货
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> AddProductReceipt(AddOrUpdProductReceiptReq obj)
        {
            var result = new TableData();
            try
            {
                result=await _productReceiptApp.ProductReceiptHandle(obj);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }

    }
}
