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
using Serilog;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OpenAuth.WebApi.Controllers.Sap
{
    /// <summary>
    /// 业务伙伴
    /// </summary>
    [Route("api/Sap/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Sap")]
    public class BusinessPartnerController : ControllerBase
    {
        private readonly BusinessPartnerApp _businessPartnerApp;
        private readonly HttpClienService _httpClienService;
        public BusinessPartnerController(BusinessPartnerApp businessPartnerApp, HttpClienService httpClienService)
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
                result = await _businessPartnerApp.Load(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{cardCode}, 错误：{result.Message}");
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
                Log.Logger.Error($"地址：{Request.Path}，参数：{CardCode}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 验证是否存在客户（新威智能App）
        /// </summary>
        /// <param name="cardCode">客户编号</param>
        /// <param name="custName">客户名称</param>
        /// <param name="userName">帐户</param>
        /// <param name="passWord">密码</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> AppGetCustomerCode(string cardCode, string custName, string userName, string passWord,int appUserId)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("cardCode", cardCode);
                parameters.Add("custName", custName);
                parameters.Add("userName", userName);
                parameters.Add("passWord", passWord);
                parameters.Add("appUserId", appUserId);
                var r = await _httpClienService.Get(parameters, "api/user/UserManage/AppGetCustomerCode");
                result = JsonConvert.DeserializeObject<TableData>(r);
                //result = await _businessPartnerApp.AppGetCustomerCode(cardCode, custName);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{cardCode},{custName},{userName},{passWord},{appUserId}， 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 生成客户二维码
        /// </summary>
        /// <param name="cardCode"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GenerateQRCode([FromQuery]GenerateQRCodeReq req)
        {
            TableData res = new TableData();
            res.Data = await _businessPartnerApp.GenerateQRCode(req);
            return res;
        }

        /// <summary>
        /// 转为共享伙伴
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> AddSharingPartner(QueryBusinessPartnerListReq req)
        {
            Response res = new Response();
            await _businessPartnerApp.AddSharingPartner(req);
            return res;
        }
        /// <summary>
        /// 转为普通伙伴
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> DeleteSharingPartner(QueryBusinessPartnerListReq req)
        {
            Response res = new Response();
            await _businessPartnerApp.DeleteSharingPartner(req);
            return res;
        }
    }
}
