using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Jobs
{
    /// <summary>
    /// App打卡提醒
    /// </summary>
    public class AppClockMessageNoticJob : IJob
    {
        private readonly OpenJobApp _openJobApp;
        private readonly AttendanceClockApp _attendanceClockApp;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="openJobApp"></param>
        /// <param name="attendanceClockApp"></param>
        public AppClockMessageNoticJob(OpenJobApp openJobApp, AttendanceClockApp attendanceClockApp)
        {
            _openJobApp = openJobApp;
            _attendanceClockApp = attendanceClockApp;
        }
        /// <summary>
        /// App打卡推送提醒
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);
            await _attendanceClockApp.AppClockMessageNotic();
            _openJobApp.RecordRun(jobId);
        }
    }
}
