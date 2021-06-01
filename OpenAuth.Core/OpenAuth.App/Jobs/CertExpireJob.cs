using OpenAuth.App.Nwcali;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Jobs
{
    public class CertExpireJob : IJob
    {
        private readonly ServiceOrderApp _serviceOrderApp;
        private readonly OpenJobApp _openJobApp;
        private readonly CertinfoApp _certinfoApp;
        public CertExpireJob(OpenJobApp openJobApp, ServiceOrderApp serviceOrderApp, CertinfoApp certinfoApp)
        {
            _openJobApp = openJobApp;
            _serviceOrderApp = serviceOrderApp;
            _certinfoApp = certinfoApp;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);

            await _certinfoApp.PushCertGuidToApp();

            _openJobApp.RecordRun(jobId);
        }
    }
}
