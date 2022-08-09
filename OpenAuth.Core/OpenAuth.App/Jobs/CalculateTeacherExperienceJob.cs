using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Jobs
{
    /// <summary>
    /// 讲师经验只计算
    /// </summary>
    public class CalculateTeacherExperienceJob : IJob
    {
        private readonly OpenJobApp _openJobApp;
        private readonly LecturerApp _lecturerApp;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="openJobApp"></param>
        /// <param name="lecturerApp"></param>
        public CalculateTeacherExperienceJob(OpenJobApp openJobApp, LecturerApp lecturerApp)
        {
            _openJobApp = openJobApp;
            _lecturerApp = lecturerApp;
        }
        /// <summary>
        /// 讲师经验只计算
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);
            await _lecturerApp.CalculateTeacherExperience();
            _openJobApp.RecordRun(jobId);
        }
    }
}
