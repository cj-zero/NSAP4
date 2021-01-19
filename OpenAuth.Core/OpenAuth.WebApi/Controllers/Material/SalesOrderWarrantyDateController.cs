using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Material;
using OpenAuth.App.Material.Request;
using OpenAuth.App.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Material
{
    [Route("api/Material/[controller]/[action]")]
    [ApiController]
    public class SalesOrderWarrantyDateController : ControllerBase
    {
        private readonly SalesOrderWarrantyDateApp _app;
        public SalesOrderWarrantyDateController(SalesOrderWarrantyDateApp app)
        {
            _app = app;
        }

        /// <summary>
        /// 加载销售单列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> Load([FromQuery] SalesOrderWarrantyDateReq request)
        {
            var result = new TableData();
            try
            {
                return await _app.Load(request);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> UpDate(AddOrUpdatesalesorderwarrantydateReq request)
        {
            var result = new Response();
            try
            {
                await _app.UpDate(request);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 审批
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Approval(AddOrUpdatesalesorderwarrantydateReq request)
        {
            var result = new Response();
            try
            {
                await _app.Approval(request);
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
