using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Sap.BusinessPartner;
using OpenAuth.App.Sap.Request;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OpenAuth.WebApi.Controllers.Sap
{
    /// <summary>
    /// 业务伙伴
    /// </summary>
    [Route("api/Sap/[controller]/[action]")]
    [ApiController]
    public class BusinessPartnerController : ControllerBase
    {
        private readonly BusinessPartnerApp _businessPartnerApp;

        public BusinessPartnerController(BusinessPartnerApp businessPartnerApp)
        {
            _businessPartnerApp = businessPartnerApp;
        }
        /// <summary>
        /// 业务伙伴分页查询
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> Load([FromQuery]QueryBusinessPartnerListReq req)
        {
            var result = new TableData();
            try
            {
                result = await _businessPartnerApp.Get(req);
            }
            catch (Exception ex) 
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 服务呼叫需要用到的客户信息
        /// </summary>
        /// <param name="cardCode">客户编号</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response<BusinessPartnerDetailsResp>> GetCardInfoForServe(string cardCode)
        {
            var result = new Response<BusinessPartnerDetailsResp>();
            try
            {
                result.Result = await _businessPartnerApp.GetDetails(cardCode);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }
    }
}
