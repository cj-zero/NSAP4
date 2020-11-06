using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Material;
using OpenAuth.App.Material.Request;
using OpenAuth.App.Response;

namespace OpenAuth.WebApi.Controllers.Material
{
    /// <summary>
    /// 报价表
    /// </summary>
    [Route("api/Material/[controller]/[action]")]
    [ApiController]
    public class QuotationController : ControllerBase
    {
        private readonly QuotationApp _app;
        public QuotationController(QuotationApp app)
        {
            _app = app;
        }


        /// <summary>
        /// 加载服务单列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetServiceOrderList([FromQuery]QueryQuotationListReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.GetServiceOrderList(request);
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
