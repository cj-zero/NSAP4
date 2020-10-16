using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Sap.BusinessPartner;

namespace NSAP.App.WebApi.Controllers
{
    /// <summary>
    /// 售后接口服务
    /// </summary>
    [Route("api/user/[controller]/[action]")]
    [ApiController]
    public class UserManageController : Controller
    {
        private readonly AppUserBindApp _app;
        private readonly BusinessPartnerApp _businessPartnerApp;
        public UserManageController(AppUserBindApp app, BusinessPartnerApp businessPartnerApp)
        {
            _app = app;
            _businessPartnerApp = businessPartnerApp;
        }

        /// <summary>
        /// 添加绑定记录
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> AddOrUpdateAppUserBind(AddOrUpdateAppUserBindReq obj)
        {
            var result = new Response();
            try
            {
                await _app.AddOrUpdateAppUserBind(obj);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 获取绑定结果
        /// </summary>
        [HttpGet]
        public async Task<TableData> GetBindInfo(int AppUserId)
        {
            var result = new TableData();
            try
            {
                result = await _app.GetBindInfo(AppUserId);
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
        /// <param name="custName">客户名称</param>
        /// <param name="userName">帐户</param>
        /// <param name="passWord">密码</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> AppGetCustomerCode(string cardCode, string custName, string userName, string passWord)
        {
            var result = new TableData();
            try
            {
                result = await _businessPartnerApp.AppGetCustomerCode(cardCode, custName, userName, passWord);
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
