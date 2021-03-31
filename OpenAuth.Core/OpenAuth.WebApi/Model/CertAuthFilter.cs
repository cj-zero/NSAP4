using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OpenAuth.App;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;

namespace OpenAuth.WebApi.Model
{
    public class CertAuthFilter : IActionFilter
    {
        private readonly IAuth _authUtil;
        private readonly SysLogApp _logApp;

        public CertAuthFilter(IAuth authUtil, SysLogApp logApp)
        {
            _authUtil = authUtil;
            _logApp = logApp;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var description =
                (Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)context.ActionDescriptor;
            Dictionary<string, object> actionArguments = (Dictionary<string, object>)context.ActionArguments;
            Parameter parameter = DicToObject<Parameter>(actionArguments);
            var Controllername = description.ControllerName.ToLower();
            var Actionname = description.ActionName.ToLower();

            //匿名标识
            var authorize = description.MethodInfo.GetCustomAttribute(typeof(AllowAnonymousAttribute));
            if (authorize != null)
            {
                return;
            }

            var loginInfo = _authUtil.GetLoginInfo();
            string appKey = loginInfo.Token;
            Dictionary<string, string> keyValues = new Dictionary<string, string>();
            keyValues.Add("SerialNumber", parameter.serialNumber);
            keyValues.Add("TimeSpan", parameter.timespan);
            keyValues.Add("Token", appKey);
            //获取签名进行校验
            string sign = SignHelper.Sign(keyValues);

            if (sign == parameter.sign)
            {
                return;
            }
            else
            {
                context.HttpContext.Response.StatusCode = 401;
                context.Result = new JsonResult(new Response
                {
                    Code = 401,
                    Message = "很抱歉，您没有权限"
                });
            }

            _logApp.Add(new SysLog
            {
                Content = $"用户访问",
                Href = $"{Controllername}/{Actionname}",
                CreateName = _authUtil.GetUserName(),
                CreateId = _authUtil.GetCurrentUser()?.User?.Id ?? "",
                TypeName = "访问日志"
            });
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            return;
        }


        public class Parameter
        {
            public string serialNumber { get; set; }

            public string timespan { get; set; }

            public string sign { get; set; }
        }

        /// <summary>
        /// 字典类型转化为对象
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public T DicToObject<T>(Dictionary<string, object> dic) where T : new()
        {
            var md = new T();
            foreach (var d in dic)
            {
                try
                {
                    md.GetType().GetProperty(d.Key).SetValue(md, d.Value);
                }
                catch (Exception e)
                {

                }
            }
            return md;
        }
    }
}
