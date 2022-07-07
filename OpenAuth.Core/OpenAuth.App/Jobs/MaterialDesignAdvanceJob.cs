using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OpenAuth.App.Material;
using Quartz;

namespace OpenAuth.App.Jobs
{
    public class MaterialDesignAdvanceJob : IJob
    {
        private readonly OpenJobApp _openJobApp;
        private readonly MaterialDesignApp _app;
        public MaterialDesignAdvanceJob(OpenJobApp openJobApp, MaterialDesignApp app)
        {
            _openJobApp = openJobApp;
            _app = app;
        }

        /// <summary>
        /// 同步物料设计进度
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);

            await _app.SyncMaterialDesignAdvance();

            _openJobApp.RecordRun(jobId);
        }
    }
}
