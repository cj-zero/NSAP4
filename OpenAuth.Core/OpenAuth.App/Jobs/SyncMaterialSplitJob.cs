using OpenAuth.App.Material;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Jobs
{
    public class SyncMaterialSplitJob : IJob
    {
        private readonly InternalContactApp _internalContactApp;
        private readonly OpenJobApp _openJobApp;
        public SyncMaterialSplitJob(InternalContactApp internalContactApp, OpenJobApp openJobApp)
        {
            _internalContactApp = internalContactApp;
            _openJobApp = openJobApp;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);

            await _internalContactApp.MaterialSplit();

            _openJobApp.RecordRun(jobId);
        }
    }
}
