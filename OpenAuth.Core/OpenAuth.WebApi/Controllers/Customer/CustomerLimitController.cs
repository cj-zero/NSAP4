using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenAuth.App.Customer;
using OpenAuth.App.Customer.Request;
using OpenAuth.App.Response;

namespace OpenAuth.WebApi.Controllers.Customer
{
    /// <summary>
    /// 最大客户限制相关接口
    /// </summary>
    [Route("api/CustomerLimit/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Customer")]
    public class CustomerLimitController : ControllerBase
    {
        private readonly CustomerLimitApp _customerLimitApp;
        public CustomerLimitController(CustomerLimitApp customerLimitApp)
        {
            _customerLimitApp = customerLimitApp;
        }

        /// <summary>
        /// 新增or修改组规则
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Infrastructure.Response> AddGroupRules(AddOrUpdateGroupRulesReq req)
        {
            var response = new Infrastructure.Response();

            try
            {
                response = await _customerLimitApp.AddOrUpdateGroupRule(req);
            }
            catch (Exception ex)
            {
                response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
                response.Code = 500;
            }

            return response;
        }

        /// <summary>
        /// 查询组规则列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> GetGroupRules(QueryCustomerLimitReq req)
        {
            var result = new TableData();

            try
            {
                result = await _customerLimitApp.GetGroupRules(req);
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message ?? "";
                result.Code = 500;
            }

            return result;
        }

        /// <summary>
        /// 删除组规则
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Infrastructure.Response> DeleteGroupRule(DeleteGroupRuleReq req)
        {
            var response = new Infrastructure.Response();

            response = await _customerLimitApp.DeleteGroupRule(req);

            return response;
        }

        /// <summary>
        /// 启用or禁用组规则
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Infrastructure.Response> EnableGroupRole(EnableGroupRoleReq req)
        {
            var response = new Infrastructure.Response();

            try
            {
                response = await _customerLimitApp.EnableGroupRole(req);
            }
            catch (Exception ex)
            {
                response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
                response.Code = 500;
            }

            return response;
        }

        /// <summary>
        /// 新增or修改用户组用户
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Infrastructure.Response> UpdateGroupUser(AddOrUpdateGroupUsersReq req)
        {
            var response = new Infrastructure.Response();

            try
            {
                response = await _customerLimitApp.UpdateGroupUser(req);
            }
            catch (Exception ex)
            {
                response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
                response.Code = 500;
            }

            return response;
        }
    }
}
