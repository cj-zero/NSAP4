using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Jobs
{
    /// <summary>
    /// 烤机结果校验
    /// </summary>
    public class DeviceTestCheckResultJob : IJob
    {
        private readonly OpenJobApp _openJobApp;
        private readonly StepVersionApp _stepVersionApp;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="openJobApp"></param>
        /// <param name="stepVersionApp"></param>
        public DeviceTestCheckResultJob(OpenJobApp openJobApp, StepVersionApp stepVersionApp)
        {
            _openJobApp = openJobApp;
            _stepVersionApp = stepVersionApp;
        }
        /// <summary>
        /// 烤机结果校验
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);
            await _stepVersionApp.DeviceTestCheckResult();
            _openJobApp.RecordRun(jobId);
        }
    }
}
