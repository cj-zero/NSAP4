using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenAuth.App.Customer;
using OpenAuth.App.Response;
using OpenAuth.App.Customer.Request;
using OpenAuth.Repository.Domain;

namespace OpenAuth.WebApi.Controllers.Customer
{
    /// <summary>
    /// 客户白名单相关接口
    /// </summary>
    [Route("api/customer/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Customer")]
    public class CustomerListController : ControllerBase
    {
        private readonly CustomerListApp _customerListApp;
        public CustomerListController(CustomerListApp customerListApp)
        {
            _customerListApp = customerListApp;
        }

        /// <summary>
        /// 查询客户列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> GetCustomers(QueryCustomerListReq req)
        {
            var result = new TableData();

            try
            {
                result = await _customerListApp.GetCustomers(req);
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message ?? "";
                result.Code = 500;
            }

            return result;
        }

        /// <summary>
        /// 新增客户白名单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Infrastructure.Response> AddCustomer(List<AddCustomerListReq> model)
        {
            var result = new Infrastructure.Response();

            try
            {
                result = await _customerListApp.AddCustomer(model);
            }
            catch (Exception ex)
            {
                result.Message = ex.InnerException?.Message ?? ex.Message ?? "";
                result.Code = 500;
            }

            return result;
        }

        /// <summary>
        /// 获取黑白名单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetWhiteOrBlackList([FromQuery] QueryWhiteOrBlackListReq req)
        {
            var result = new TableData();

            try
            {
                result = await _customerListApp.GetWhiteOrBlackList(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message ?? "";
            }

            return result;
        }


        /// <summary>
        /// 删除黑白名单客户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Infrastructure.Response> DeleteCustomer(DeleteCustomerListReq req)
        {
            var result = new Infrastructure.Response();

            try
            {
                result = await _customerListApp.DeleteCustomer(req.Id);
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
