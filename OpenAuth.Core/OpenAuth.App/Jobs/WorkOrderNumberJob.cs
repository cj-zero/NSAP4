using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Jobs
{
    public class WorkOrderNumberJob : IJob
    {
        private readonly ServiceOrderApp _serviceOrderApp;
        private readonly OpenJobApp _openJobApp;
        public WorkOrderNumberJob(OpenJobApp openJobApp, ServiceOrderApp serviceOrderApp)
        {
            _openJobApp = openJobApp;
            _serviceOrderApp = serviceOrderApp;
        }
        /// <summary>
        /// 处理未生成工单号
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);
            //todo:这里可以加入自己的自动任务逻辑

            await _serviceOrderApp.UpDateWorkOrderNumber();

            _openJobApp.RecordRun(jobId);
        }
    }
}
