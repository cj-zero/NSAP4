using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Customer;
using OpenAuth.App.Customer.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Customer
{
    /// <summary>
    /// 公海设置相关接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Customer")]
    public class CustomerSeaConfigController : ControllerBase
    {
        private readonly CustomerSeaConfApp _customerSeaConfApp;
        public CustomerSeaConfigController(CustomerSeaConfApp customerSeaConfApp)
        {
            _customerSeaConfApp = customerSeaConfApp;
        }

        /// <summary>
        /// 自动放入公海设置
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Infrastructure.Response> AutoPutInCustomerSea(AutoPutInCustomerSeaObjectReq req)
        {
            var response = new Infrastructure.Response();

            try
            {
                response = await _customerSeaConfApp.AutoPutInCustomerSea(req);
            }
            catch (Exception ex)
            {
                response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
                response.Code = 500;
            }

            return response;
        }

        /// <summary>
        /// 公海回收机制设置
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Infrastructure.Response> CustomerSeaRecovery(CustomerSeaRecoveryObjectReq req)
        {
            var response = new Infrastructure.Response();

            try
            {
                response = await _customerSeaConfApp.CustomerSeaRecovery(req);
            }
            catch (Exception ex)
            {
                response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
                response.Code = 500;
            }

            return response;
        }

        /// <summary>
        /// 公海认领规则
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Infrastructure.Response> ClaimRules(ClaimRulesObjectReq req)
        {
            var response = new Infrastructure.Response();

            try
            {
                response = await _customerSeaConfApp.ClaimRules(req);
            }
            catch (Exception ex)
            {
                response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
                response.Code = 500;
            }

            return response;
        }

        /// <summary>
        /// 主动掉入公海限制
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Infrastructure.Response> AutomaticLimit(AutomaticObjectReq req)
        {
            var response = new Infrastructure.Response();

            try
            {
                response = await _customerSeaConfApp.AutomaticLimit(req);
            }
            catch (Exception ex)
            {
                response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
                response.Code = 500;
            }

            return response;
        }

        /// <summary>
        /// 掉入公海后抢回限制
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Infrastructure.Response> GetBackLimit(GetBackLimitObjectReq req)
        {
            var response = new Infrastructure.Response();

            try
            {
                response = await _customerSeaConfApp.GetBackLimit(req);
            }
            catch (Exception ex)
            {
                response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
                response.Code = 500;
            }

            return response;
        }

        /// <summary>
        /// 根据字段修改启用字段
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Infrastructure.Response> EnableObject(EnableObject req)
        {
            var response = new Infrastructure.Response();

            try
            {
                response = await _customerSeaConfApp.EnableObject(req);
            }
            catch (Exception ex)
            {
                response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
                response.Code = 500;
            }

            return response;
        }

        /// <summary>
        /// 查询公海设置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetCustomerSeaConfig()
        {
            var result = new TableData();

            try
            {
                result = await _customerSeaConfApp.GetCustomerSeaConfig();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex?.InnerException?.Message ?? ex?.Message ?? "";
            }

            return result;
            //return await _customerSeaConfApp.GetCustomerSeaConfig();
        }
    }
}
