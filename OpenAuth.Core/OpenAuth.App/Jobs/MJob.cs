using OpenAuth.App.ClientRelation;
using OpenAuth.App.Material;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Jobs
{
    public class MJob : IJob
    {
        private readonly OpenJobApp _openJobApp;
        private readonly MaterialDesignApp  _materialDesignApp;

        public MJob(OpenJobApp openJobApp, MaterialDesignApp  materialDesignApp)
        {
            _openJobApp = openJobApp;
            _materialDesignApp = materialDesignApp;
        }


        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);
            await _materialDesignApp.MByJob();
            _openJobApp.RecordRun(jobId);
        }
    }
}
