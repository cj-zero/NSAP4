using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Meeting.ModelDto
{
    public class MeetingUserHistoryDto
    {
        /// <summary>
        /// 会议名称
        /// </summary>
        public string  MeetingName { get; set; }
        /// <summary>
        /// 报名人
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 报名时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 展会开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 展会结束时间
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 1：国内，2：国外
        /// </summary>
        public int AddressType { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
    }
}
