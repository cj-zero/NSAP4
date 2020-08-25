using Infrastructure;
using NUnit.Framework.Constraints;
using OpenAuth.App.SignalR;
using OpenAuth.App.SignalR.Request;
using OpenAuth.Repository;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Jobs
{
    public class PendingNumberJob : IJob
    {

        private readonly SignalRMessageApp _signalrmessage;
        private readonly ServiceOrderApp _serviceOrderApp;
        private readonly OpenJobApp _openJobApp;
        public PendingNumberJob(OpenJobApp openJobApp, SignalRMessageApp signalrmessage, ServiceOrderApp serviceOrderApp)
        {
            _openJobApp = openJobApp;
            _signalrmessage = signalrmessage;
            _serviceOrderApp = serviceOrderApp;
        }
        /// <summary>
        /// 推送待处理呼叫服务数量和待派单数量
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);
            //todo:这里可以加入自己的自动任务逻辑
            var message = new { ServiceOrderCount = await _serviceOrderApp.GetServiceOrderCount(), ServiceWorkOrderCount = await _serviceOrderApp.GetServiceWorkOrderCount() };
                
            
            await _signalrmessage.SendPendingNumber(new SendRoleMessageReq { Role="售后主管",RoleTwo= "呼叫中心", Message= message.ToJson() });

            _openJobApp.RecordRun(jobId);
        }
    }
}
