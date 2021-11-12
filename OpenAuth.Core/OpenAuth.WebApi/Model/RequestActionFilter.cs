using System;
using Microsoft.AspNetCore.Mvc.Filters;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Domain;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.IO;
using OpenAuth.App;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Model
{
    public class RequestActionFilter : IAsyncActionFilter
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
        /// 当有异步方法的时候,会优先走异步方法,同步的不会执行(因此OnActionExecuting和OnActionExecuted都不会执行)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var descriptor = (Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)context.ActionDescriptor;
            var controllerName = descriptor.ControllerName;
            var actionName = descriptor.ActionName;
            var parameter = JsonConvert.SerializeObject(context.ActionArguments);
            var currentUser = _auth.GetCurrentUser()?.User?.Id ?? "";

            var log = new RequestActionLog
            {
                ActionName = $"{controllerName}/{actionName}",
                Parameter = parameter,
                RequestUser = currentUser,
                RequestTime = DateTime.Now
            };

            var resultContext = await next();

            if (resultContext.Exception != null)
            {
                log.ApiResult = resultContext.Exception?.Message;
            }
            else if (resultContext.Result != null)
            {
                try
                {
                    var resultValue = ((ObjectResult)resultContext.Result).Value;
                    log.ApiResult = JsonConvert.SerializeObject(resultValue);
                }
                catch(Exception ex)
                {
                    log.ApiResult = ex.Message;
                }
            }

            _requestActionLogApp.Add(log);
        }
    }
}
