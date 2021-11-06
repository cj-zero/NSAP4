using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Meeting.Request
{
    public class MeetingUserHistoryReq : PageReq
    {
        /// <summary>
        /// 会议名称
        /// </summary>
        public string MeetingName { get; set; }
        /// <summary>
        /// 1：国内，2：国外
        /// </summary>
        public int AddressType { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime { get; set; }
        /// <summary>
        /// 用户Id
        /// </summary>
        public int UserId { get; set; }
    }
}
