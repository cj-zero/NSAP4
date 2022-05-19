using OpenAuth.App.Customer;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Jobs
{
    /// <summary>
    /// 运行定时任务,把符合掉落规则的客户拉入公海
    /// </summary>
    public class CustomerSeaJob : IJob
    {
        private readonly OpenJobApp _openJobApp;
        private readonly CustomerLimitApp _customerLimitApp;

        public CustomerSeaJob(OpenJobApp openJobApp, CustomerLimitApp customerLimitApp)
        {
            _openJobApp = openJobApp;
            _customerLimitApp = customerLimitApp;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);
            await _customerLimitApp.AsyncCustomerStatusService();
            _openJobApp.RecordRun(jobId);
        }
    }

    /// <summary>
    /// 运行定时任务,把符合规则的客户回收(指的是掉入公海后，被领取的客户)
    /// </summary>
    public class RecoveryCustomer : IJob
    {
        private readonly OpenJobApp _openJobApp;
        private readonly CustomerLimitApp _customerLimitApp;
        public RecoveryCustomer(OpenJobApp openJobApp, CustomerLimitApp customerLimitApp)
        {
            _openJobApp = openJobApp;
            _customerLimitApp = customerLimitApp;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);
            await _customerLimitApp.RecoveryCustomer();
            _openJobApp.RecordRun(jobId);
        }
    }

    /// <summary>
    /// 运行定时任务,向即将掉入公海的客户业务员发送提醒消息
    /// </summary>
    public class PushMessage : IJob
    {
        private readonly OpenJobApp _openJobApp;
        private readonly CustomerLimitApp _customerLimitApp;
        public PushMessage(OpenJobApp openJobApp, CustomerLimitApp customerLimitApp)
        {
            _openJobApp = openJobApp;
            _customerLimitApp = customerLimitApp;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);
            await _customerLimitApp.PushMessage();
            _openJobApp.RecordRun(jobId);
        }
    }
}
