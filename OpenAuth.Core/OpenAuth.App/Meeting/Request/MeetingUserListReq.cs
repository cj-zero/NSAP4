using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Meeting.Request
{
    public class MeetingUserListReq:PageReq
    {
        /// <summary>
        /// 会议Id
        /// </summary>
    
        public int MeetingId { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public int Name { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime { get; set; }
    }
}
