using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        private readonly HttpClienService _httpClienService;
        public BusinessPartnerController(BusinessPartnerApp businessPartnerApp,HttpClienService httpClienService)
        {
            _businessPartnerApp = businessPartnerApp;
            _httpClienService = httpClienService;
        }
        /// <summary>
        /// 业务伙伴分页查询
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> Load([FromQuery] QueryBusinessPartnerListReq req)
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

        /// <summary>
        /// 服务呼叫查询单个客户信息
        /// </summary>
        /// <param name="CardCode">客户编号</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetBusinessAssociate(string CardCode)
        {
            var result = new TableData();
            try
            {
                return await _businessPartnerApp.GetBusinessAssociate(CardCode);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 验证是否存在客户（新威智能App）
        /// </summary>
        /// <param name="cardCode">客户编号</param>
        /// /// <param name="custName">客户名称</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> AppGetCustomerCode(string cardCode, string custName)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("cardCode", cardCode);
                parameters.Add("custName", custName);
                var r = await _httpClienService.Get(parameters, "api/user/UserManage/AppGetCustomerCode");
                result = JsonConvert.DeserializeObject<TableData>(r);
                //result = await _businessPartnerApp.AppGetCustomerCode(cardCode, custName);
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
