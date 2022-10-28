using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Jobs
{
    public class CreateNwcailFilejob : IJob
    {
        private readonly OpenJobApp _openJobApp;
        private readonly CertinfoApp _certinfoApp;

        public CreateNwcailFilejob(OpenJobApp openJobApp, CertinfoApp certinfoApp)
        {
            _openJobApp = openJobApp;
            _certinfoApp = certinfoApp;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);
            await _certinfoApp.CreateNwcailFileHelper();
            _openJobApp.RecordRun(jobId);
        }
    }
}
