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
            Log.Logger.Error(context.Exception, context.Exception.Message);
            if (context.ExceptionHandled == false)
            {
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