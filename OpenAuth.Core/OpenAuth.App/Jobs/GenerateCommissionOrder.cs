using OpenAuth.App.Material;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Jobs
{
    public class GenerateCommissionOrder : IJob
    {
        private readonly OpenJobApp _openJobApp;
        private readonly QuotationApp _quotationApp;

        public GenerateCommissionOrder(OpenJobApp openJobApp, QuotationApp quotationfoApp)
        {
            _openJobApp = openJobApp;
            _quotationApp = quotationfoApp;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);
            await _quotationApp.GenerateCommissionSettlement();
            _openJobApp.RecordRun(jobId);
        }
    }
}
