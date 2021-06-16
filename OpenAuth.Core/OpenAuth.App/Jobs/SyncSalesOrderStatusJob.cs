using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OpenAuth.App.Material;
using Quartz;

namespace OpenAuth.App.Jobs
{
    public class SyncSalesOrderStatusJob : IJob
    {
        private readonly OpenJobApp _openJobApp;
        private readonly QuotationApp _quotationApp;
        public SyncSalesOrderStatusJob(OpenJobApp openJobApp, QuotationApp quotationApp)
        {
            _openJobApp = openJobApp;
            _quotationApp = quotationApp;
        }

        /// <summary>
        /// 同步销售订单状态
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);

            await _quotationApp.SyncSalesOrderStatus();

            _openJobApp.RecordRun(jobId);
        }
    }
}
