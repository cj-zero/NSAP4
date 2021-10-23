using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Meeting.Request
{
    /// <summary>
    /// 调度功能接受类
    /// </summary>
    public class SchedulingReq
    {
        /// <summary>
        /// 旧会议ID
        /// </summary>
        public int oldMeetingId { get; set; }
        /// <summary>
        /// 会议ID
        /// </summary>
        public int MeetingId { get; set; }
        /// <summary>
        /// 调度原因
        /// </summary>
        public string Reason { get; set; }
        public List<Schedulinger> SchedulingerList { get; set; }

    }
    public class Schedulinger
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 报名人
        /// </summary>
        public string Name { get; set; }
    }
}
