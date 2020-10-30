using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App.Serve;

namespace OpenAuth.WebApi.Controllers.Serve
{
    [Route("api/serve/[controller]/[action]")]
    [ApiController]
    public class ServiceOrderPushNotificationController : ControllerBase
    {
        private readonly ServiceOrderPushNotification _serviceorderpushnotification;

        public ServiceOrderPushNotificationController(ServiceOrderPushNotification serviceorderpushnotification)
        {
            _serviceorderpushnotification = serviceorderpushnotification;
        }
        #region  推送消息
        /// <summary>
        /// Web获取未读消息个数
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response> GetMessageWeb()
        {
            var result = new Response();
            try
            {
                await _serviceorderpushnotification.GetMessageWeb();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 推送未处理和待派单消息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Response> SendPendingNumber()
        {
            var result = new Response();
            try
            {
                await _serviceorderpushnotification.SendPendingNumber();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }
        #endregion
    }
}
