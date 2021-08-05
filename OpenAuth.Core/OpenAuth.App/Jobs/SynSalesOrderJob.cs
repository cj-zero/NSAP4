using OpenAuth.App.Material;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAuth.App.Jobs
{
    public class SynSalesOrderJob : IJob
    {
        private readonly OpenJobApp _openJobApp;
        private readonly SalesOrderWarrantyDateApp _salesOrderWarrantyDateApp;
        static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);//用信号量代替锁
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
            await semaphoreSlim.WaitAsync();
            try
            {
                _salesOrderWarrantyDateApp.SynchronizationSalesOrder();
            }
            finally
            {
                semaphoreSlim.Release();
            }
            _openJobApp.RecordRun(jobId);
        }
    }
}
