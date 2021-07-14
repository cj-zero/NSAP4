using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NetOffice.Extensions.Invoker;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
                if (new Regex("^[\u4E00-\u9FA5\u3002|\uff1f|\uff01|\uff0c|\u3001|\uff1b|\uff1a|\u201c|\u201d|\u2018|\u2019|\uff08|\uff09|\u300a|\u300b|\u3008|\u3009|\u3010|\u3011|\u300e|\u300f|\u300c|\u300d|\ufe43|\ufe44|\u3014|\u3015|\u2026|\u2014|\uff5e|\ufe4f|\uffe5]{0,}$").IsMatch(exception.Message))
                {
                    //写入日志
                    Log.Logger.Warning($"报错地址:{url},请求方式：{method},参数:{parameter},异常描述：{exception.Message},堆栈信息：{exception.StackTrace}");
                }
                else
                {
                    //写入日志
                    Log.Logger.Error($"报错地址:{url},请求方式：{method},参数:{parameter},异常描述：{exception.Message},堆栈信息：{exception.StackTrace}");
                }
                
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