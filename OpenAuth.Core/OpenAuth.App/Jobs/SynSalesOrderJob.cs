using OpenAuth.App.Material;
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
        private readonly SalesOrderWarrantyDateApp _salesOrderWarrantyDateApp;
        public SynSalesOrderJob(OpenJobApp openJobApp, SalesOrderWarrantyDateApp salesOrderWarrantyDateApp)
        {
            _openJobApp = openJobApp;
            _salesOrderWarrantyDateApp = salesOrderWarrantyDateApp;
        }
        /// <summary>
        /// 定时同步销售订单保修时间到4.0
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);
            //todo:这里可以加入自己的自动任务逻辑
            await _salesOrderWarrantyDateApp.SynchronizationSalesOrder();

            _openJobApp.RecordRun(jobId);
        }
    }
}
