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
        /// 新增组规则
        /// </summary>
        /// <returns></returns>
        public async Task<Infrastructure.Response> AddGroupRules(AddOrUpdateGroupRulesReq req)
        {
            var response = new Infrastructure.Response();

            try
            {
                response = await _customerLimitApp.AddGroupRule(req);
            }
            catch (Exception ex)
            {
                response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
                response.Code = 500;
            }

            return response;
        }

        /// <summary>
        /// 修改组规则
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Infrastructure.Response> UpdateGroupRules(AddOrUpdateGroupRulesReq req)
        {
            var response = new Infrastructure.Response();

            try
            {
                response = await _customerLimitApp.UpdateGroupRule(req);
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
        /// 向规则组添加用户
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Infrastructure.Response> AddGroupUser(AddOrDeleteGroupUsersReq req)
        {
            var response = new Infrastructure.Response();

            try
            {
                response = await _customerLimitApp.AddGroupUser(req);
            }
            catch (Exception ex)
            {
                response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
                response.Code = 500;
            }

            return response;
        }

        /// <summary>
        /// 删除规则组用户
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Infrastructure.Response> DeleteGroupUser(AddOrDeleteGroupUsersReq req)
        {
            var response = new Infrastructure.Response();

            try
            {
                response = await _customerLimitApp.DeleteGroupUser(req);
            }
            catch (Exception ex)
            {
                response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
                response.Code = 500;
            }

            return response;
        }

        /// <summary>
        /// 获取销售员信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> GetSlpInfos(QuerySlpReq req)
        {
            var response = new TableData();

            try
            {
                response = await _customerLimitApp.GetSlpInfos(req);
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
        //[HttpPost]
        //public async Task<Infrastructure.Response> UpdateGroupUser(AddOrUpdateGroupUsersReq req)
        //{
        //    var response = new Infrastructure.Response();

        //    try
        //    {
        //        response = await _customerLimitApp.UpdateGroupUser(req);
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
        //        response.Code = 500;
        //    }

        //    return response;
        //}

        /// <summary>
        /// 测试同步数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task TestAsync()
        {
            await _customerLimitApp.AsyncCustomerStatusService();
            //await _customerLimitApp.RecoveryCustomer();
            //await _customerLimitApp.PushMessage();
        }
    }
}
