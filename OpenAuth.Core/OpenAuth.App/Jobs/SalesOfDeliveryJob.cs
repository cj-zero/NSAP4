using OpenAuth.App.Material;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Jobs
{
    public class SalesOfDeliveryJob : IJob
    {
        private readonly OpenJobApp _openJobApp;
        private readonly QuotationApp _quotationApp;
        public SalesOfDeliveryJob(OpenJobApp openJobApp, QuotationApp quotationApp)
        {
            _openJobApp = openJobApp;
            _quotationApp = quotationApp;
        }
        /// <summary>
        /// 定时维修费和差旅费交货
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);
            //todo:这里可以加入自己的自动任务逻辑
            //await _quotationApp.TimeOfDelivery();
            _openJobApp.RecordRun(jobId);
        }
    }
}
