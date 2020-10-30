using OpenAuth.App.Serve;
using OpenAuth.App.SignalR;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Jobs
{
    public class MessageJob : IJob
    {
        private readonly ServiceOrderPushNotification _serviceorderpushnotification;
        private readonly ServiceOrderApp _serviceOrderApp;
        private readonly OpenJobApp _openJobApp;
        public MessageJob(OpenJobApp openJobApp, ServiceOrderPushNotification serviceorderpushnotification)
        {
            _openJobApp = openJobApp;
            _serviceorderpushnotification = serviceorderpushnotification;
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

            await _serviceorderpushnotification.GetMessageWeb();

            _openJobApp.RecordRun(jobId);
        }
    }
}
