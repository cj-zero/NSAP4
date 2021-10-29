using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Meeting.Request
{
    public class MyCreatedLoadReq : PageReq
    {
        /// <summary>
        /// 单据号
        /// </summary>
        public int JobId { get; set; }
        /// <summary>
        /// 单据类型
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 任务名称
        /// </summary>
        public string JobName { get; set; }
        /// <summary>
        /// 展会名称
        /// </summary>
        public string MeetingName { get; set; }
        /// <summary>
        /// 展会开始时间
        /// </summary>
        public string StartTime { get; set; }
        /// <summary>
        /// 展会结束时间
        /// </summary>
        public string EndTime { get; set; }
        /// <summary>
        /// 0：国内，1：国外
        /// </summary>
        public int AddressType { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
    }
}
