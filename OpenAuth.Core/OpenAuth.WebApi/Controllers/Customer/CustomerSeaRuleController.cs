using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Customer;
using OpenAuth.App.Customer.Request;
using OpenAuth.App.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Customer
{
    /// <summary>
    /// 公海规则相关接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Customer")]
    public class CustomerSeaRuleController : ControllerBase
    {
        private readonly CustomerSeaRuleApp _customerSeaRuleApp;
        public CustomerSeaRuleController(CustomerSeaRuleApp customerSeaRuleApp)
        {
            _customerSeaRuleApp = customerSeaRuleApp;
        }

        /// <summary>
        /// 新增公海规则
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Infrastructure.Response> AddCustomerSeaRule(AddOrUpdateCustomerSeaRuleReq req)
        {
            var response = new Infrastructure.Response();

            response = await _customerSeaRuleApp.AddCustomerSeaRule(req);

            return response;
        }

        /// <summary>
        /// 获取公海规则信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> GetCustomerSeaRules(QueryCustomerSeaRulesReq req)
        {
            var result = new TableData();

            try
            {
                result = await _customerSeaRuleApp.GetCustomerSeaRules(req);
            }
            catch(Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message ?? "";
                result.Code = 500;
            }

            return result;
        }

        /// <summary>
        /// 根据id删除公海规则
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Infrastructure.Response> DeleteCustomerSeaRule(QueryCustomerSeaRulesReq req)
        {
            var response = new Infrastructure.Response();

            response = await _customerSeaRuleApp.DeleteCustomerSeaRule(req);

            return response;
        }

        /// <summary>
        /// 根据id修改公海规则
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Infrastructure.Response> UpdateCustomerSeaRule(AddOrUpdateCustomerSeaRuleReq req)
        {
            var response = new Infrastructure.Response();

            response = await _customerSeaRuleApp.UpdateCustomerSeaRule(req);

            return response;
        }

        /// <summary>
        /// 根据id启用or禁用公海规则
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Infrastructure.Response> EnableCustomerSeaRule(EnableCustomerSeaRuleReq req)
        {
            var response = new Infrastructure.Response();
            try
            {
                response = await _customerSeaRuleApp.EnableCustomerSeaRule(req);
            }
            catch(Exception ex)
            {
                response.Code = 500;
                response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
            }

            return response;
        }

        /// <summary>
        /// 获取叶子结点的部门信息列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetLeafOrgList()
        {
            var result = new TableData();

            try
            {
                result = await _customerSeaRuleApp.GetLeafOrgList();
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message ?? "";
                result.Code = 500;
            }

            return result;
        }
    }
}
