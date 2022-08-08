using OpenAuth.App.Client;
using OpenAuth.App.Customer;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Jobs
{
    /// <summary>
    /// 运行定时任务,向有跟进任务的业务员发送提醒
    /// </summary>
    public class ClientJob : IJob
    {
        private readonly OpenJobApp _openJobApp;
        private readonly ClientInfoApp _clientInfoApp;

        public ClientJob(OpenJobApp openJobApp, ClientInfoApp clientInfoApp)
        {
            _openJobApp = openJobApp;
            _clientInfoApp = clientInfoApp;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);
            await _clientInfoApp.PushMessageToSlp();
            _openJobApp.RecordRun(jobId);
        }
    }

}
