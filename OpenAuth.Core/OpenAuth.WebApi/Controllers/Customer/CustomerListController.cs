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
        [HttpPost]
        public async Task<TableData> GetWhiteOrBlackList(QueryWhiteOrBlackListReq req)
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

        /// <summary>
        /// 根据客户代码获取客户历史归属
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> GetCustomerSalerLists(QueryCustomerSalerListReq req)
        {
            var result = new TableData();

            try
            {
                result = await _customerListApp.GetCustomerSalerLists(req);
            }
            catch(Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message ?? "";
            }

            return result;
        }

        /// <summary>
        /// 获取掉入公海的客户列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> GetCustomerSeaLists(QueryCustomerSeaReq req)
        {
            var result = new TableData();

            try
            {
                result = await _customerListApp.GetCustomerSeaLists(req);
            }
            catch(Exception ex)
            {
                result.Code = 500;
                result.Message = ex?.Message ?? ex.InnerException?.Message ?? "";
            }

            return result;
        }

        /// <summary>
        /// 获取客户的详细信息
        /// </summary>
        /// <param name="cardCode">客户代码</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetCustomerDetail([FromQuery] string cardCode)
        {
            var result = new TableData();

            try
            {
                result = await _customerListApp.GetCustomerDetail(cardCode);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex?.Message ?? ex.InnerException?.Message ?? "";
            }

            return result;
        }

        /// <summary>
        /// 获取在职的销售员列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> GetSlpInfo(QuerySlpInfoReq req)
        {
            var result = new TableData();

            try
            {
                result = await _customerListApp.GetSlpInfo(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex?.Message ?? ex.InnerException?.Message ?? "";
            }

            return result;
        }

        /// <summary>
        /// 领取公海客户
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<Infrastructure.Response> ReceiveCustomer(ReceiveCustomerReq req)
        {
            var response = new Infrastructure.Response();
            try
            {
                response = await _customerListApp.ReceiveCustomerTest(req);
            }
            catch (Exception ex)
            {
                response.Code = 500;
                response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
            }

            return response;
        }

        /// <summary>
        /// 分配公海客户
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<Infrastructure.Response> DistributeCustomer(DistributeCustomerReq req)
        {
            var response = new Infrastructure.Response();

            try
            {
                response = await _customerListApp.DistributeCustomer(req);
            }
            catch (Exception ex)
            {
                response.Code = 500;
                response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
            }

            return response;
        }



        #region
        [HttpPost]
        public async Task<TableData> GetCustomerHistoryLists(QueryCustomerSalerListReq req)
        {
            var result = new TableData();

            try
            {
                result = await _customerListApp.GetCustomerHistoryLists(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex?.Message ?? ex.InnerException?.Message ?? "";
            }

            return result;
        }

        #endregion
    }
}
