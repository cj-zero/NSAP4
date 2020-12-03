using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Jobs
{
    public class SynSalesOrderJob : IJob
    {
        private readonly OpenJobApp _openJobApp;
        public SynSalesOrderJob(OpenJobApp openJobApp)
        {
            _openJobApp = openJobApp;
        }
        /// <summary>
        /// 定时同步销售订单到4.0
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);
            //todo:这里可以加入自己的自动任务逻辑



            _openJobApp.RecordRun(jobId);
        }
    }
}
