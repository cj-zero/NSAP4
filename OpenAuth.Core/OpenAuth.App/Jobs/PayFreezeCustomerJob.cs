using OpenAuth.App.PayTerm;
using OpenAuth.App.Customer;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Jobs
{
    public class PayFreezeCustomerJob :IJob
    {
        private readonly OpenJobApp _openJobApp;
        private readonly PayTermApp _payTermApp;

        public PayFreezeCustomerJob(OpenJobApp openJobApp, PayTermApp payTermApp)
        {
            _openJobApp = openJobApp;
            _payTermApp = payTermApp;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);
            await _payTermApp.SetAutoFreezeJob();
            await _payTermApp.SetDDSendMsgJob();
            _openJobApp.RecordRun(jobId);
        }
    }
}
