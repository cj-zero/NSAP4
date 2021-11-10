using System;
using Microsoft.AspNetCore.Mvc.Filters;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Domain;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.IO;
using OpenAuth.App;

namespace OpenAuth.WebApi.Model
{
    public class RequestActionFilter : IActionFilter
    {
        private readonly IAuth _auth;
        private readonly RequestActionLogApp _requestActionLogApp;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="auth">用户信息相关接口</param>
        /// <param name="requestActionLogApp">请求日志信息curd对象</param>
        public RequestActionFilter(IAuth auth, RequestActionLogApp requestActionLogApp)
        {
            _auth = auth;
            _requestActionLogApp = requestActionLogApp;
        }

        /// <summary>
        /// action执行前执行
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
        }

        /// <summary>
        /// action执行完之后执行
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            var descriptor = (Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)context.ActionDescriptor;
            var controllerName = descriptor.ControllerName;
            var actionName = descriptor.ActionName;
            var currentUser = _auth.GetCurrentUser()?.User?.Id ?? "";
            string requestBody = "";
            if (context != null)
            {
                var request = context.HttpContext.Request;
                request.Body.Position = 0;
                using (var stream = new StreamReader(request.Body))
                {
                    requestBody = stream.ReadToEnd();
                    request.Body.Position = 0;
                }
            }

            var log = new RequestActionLog
            {
                ActionName = $"{controllerName}/{actionName}",
                Parameter = requestBody,
                RequestUser = currentUser,
                RequestTime = DateTime.Now
            };

            if (context.Exception != null)
            {
                log.ApiResult = JsonConvert.SerializeObject(context.Exception.InnerException.Message);
            }
            else if(context.Result != null)
            {
                log.ApiResult = JsonConvert.SerializeObject(((ObjectResult)context.Result).Value);
            }

            _requestActionLogApp.Add(log);
        }
    }
}
