using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Jobs
{
    public class GenerateIncomeSummaryJob
    {
        private readonly OpenJobApp _openJobApp;
        private readonly ServiceOrderApp _serviceOrderApp;

        public GenerateIncomeSummaryJob(OpenJobApp openJobApp, ServiceOrderApp quotationfoApp)
        {
            _openJobApp = openJobApp;
            _serviceOrderApp = quotationfoApp;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);
            await _serviceOrderApp.GenerateIncomeSummary();
            _openJobApp.RecordRun(jobId);
        }
    }
}
