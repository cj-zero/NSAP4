using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Quartz;

namespace OpenAuth.App.Jobs
{
    public class UnSubmitReturnNoteJob : IJob
    {
        private readonly ReturnNoteApp _returnNoteApp;
        private readonly OpenJobApp _openJobApp;

        public UnSubmitReturnNoteJob(ReturnNoteApp returnNoteApp, OpenJobApp openJobApp)
        {
            _returnNoteApp = returnNoteApp;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);
            await _returnNoteApp.SendUnSubmitMessage();
            //await _returnNoteApp.SendUnSubmitCount();
            _openJobApp.RecordRun(jobId);
        }
    }
}
