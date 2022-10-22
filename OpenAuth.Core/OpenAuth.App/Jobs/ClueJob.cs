using OpenAuth.App.Client;
using OpenAuth.App.ClientRelation;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Jobs
{
    public class ClueJob : IJob
    {
        private readonly OpenJobApp _openJobApp;
        private readonly ClueApp  _clueApp;

        public ClueJob(OpenJobApp openJobApp, ClueApp  clueApp)
        {
            _openJobApp = openJobApp;
            _clueApp = clueApp;
        }


        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);
            await _clueApp.ChangeClueStatusByJob();
            _openJobApp.RecordRun(jobId);
        }


    }


}
