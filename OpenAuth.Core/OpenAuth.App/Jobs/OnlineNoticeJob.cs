using OpenAuth.App.Serve;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Jobs
{
    public class OnlineNoticeJob : IJob
    {
        private readonly OpenJobApp _openJobApp;
        private readonly RealTimeLocationPush _realTimeLocationPush;
        public OnlineNoticeJob(OpenJobApp openJobApp, RealTimeLocationPush realTimeLocationPush)
        {
            _openJobApp = openJobApp;
            _realTimeLocationPush = realTimeLocationPush;
        }

        /// <summary>
        /// 技术员上线/下线提醒Job
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);

            await _realTimeLocationPush.OnlineNotice();

            _openJobApp.RecordRun(jobId);
        }
    }
}
