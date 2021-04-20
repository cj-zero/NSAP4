using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NetOffice.Extensions.Invoker;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Exceptions
{
    public class ExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            //Log.Logger.Error(context.Exception, context.Exception.Message);
            if (context.ExceptionHandled == false)
            {
                //日志入库
                Exception exception = context.Exception;
                //报错地址
                string url = context.HttpContext.Request.Host + context.HttpContext.Request.Path;
                //报错参数
                string parameter = context.HttpContext.Request.QueryString.ToString();
                //报错请求方式
                string method = context.HttpContext.Request.Method.ToString();
                //写入日志
                Log.Logger.Error($"报错地址:{url},请求方式：{method},参数:{parameter},异常描述：{exception.Message},堆栈信息：{exception.StackTrace}");
                if (context.Exception is CommonException ex)
                {
                    context.Result = new ObjectResult(new Response
                    {
                        Code = ex.Code,
                        Message = ex.Message
                    });
                }
                else
                {
                    context.Result = new ObjectResult(new Response
                    {
                        Code = 500,
                        Message = context.Exception.Message
                    });
                }

            }
            context.ExceptionHandled = true;
        }
        /// <summary>
        /// 异步发生异常时进入
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task OnExceptionAsync(ExceptionContext context)
        {
            OnException(context);
            return Task.CompletedTask;

        }
    }
}