using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Jobs
{
    /// <summary>
    /// App签退打卡提醒
    /// </summary>
    public class AppSignOutMessageNoticJob : IJob
    {
        private readonly OpenJobApp _openJobApp;
        private readonly AttendanceClockApp _attendanceClockApp;

        public AppSignOutMessageNoticJob(OpenJobApp openJobApp, AttendanceClockApp attendanceClockApp)
        {
            _openJobApp = openJobApp;
            _attendanceClockApp = attendanceClockApp;
        }

        /// <summary>
        /// App签退打卡提醒
        /// </summary>
        public async Task Execute(IJobExecutionContext context)
        {
            var jobId = context.MergedJobDataMap.GetString(Define.JOBMAPKEY);
            await _attendanceClockApp.AppSignOutMessageNotic();
            _openJobApp.RecordRun(jobId);
        }
    }
}
