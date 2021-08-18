using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Jobs
{
    public class SyncUserJob : IJob
    {
        private SysLogApp _sysLogApp;
        private OpenJobApp _openJobApp;
        private UserManagerApp _userManagerApp;

        public SyncUserJob(SysLogApp sysLogApp, OpenJobApp openJobApp, UserManagerApp userManagerApp)
        {
            _sysLogApp = sysLogApp;
            _openJobApp = openJobApp;
            _userManagerApp = userManagerApp;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);
            await _userManagerApp.SysnERPUser();
            _openJobApp.RecordRun(jobId);
        }
    }
}
