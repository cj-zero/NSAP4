using OpenAuth.App.Serve;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Jobs
{
    /// <summary>
    /// 智慧大屏数据/消息推送Job
    /// </summary>
    public class SmartScreenJob : IJob
    {
        private readonly OpenJobApp _openJobApp;
        private readonly RealTimeLocationPush _realTimeLocationPush;

        public SmartScreenJob(OpenJobApp openJobApp, RealTimeLocationPush realTimeLocationPush)
        {
            _openJobApp = openJobApp;
            _realTimeLocationPush = realTimeLocationPush;
        }

        /// <summary>
        /// 推送智慧大屏数据/消息
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);

            await _realTimeLocationPush.PushDataMessage();

            _openJobApp.RecordRun(jobId);
        }
    }
}
