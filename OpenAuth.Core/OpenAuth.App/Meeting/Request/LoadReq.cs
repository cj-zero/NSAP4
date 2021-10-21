using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Meeting.Request
{
    public class LoadReq : PageReq
    {
        /// <summary>
        /// 展会名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 展会开始时间
        /// </summary>
        public string StartTime { get; set; }
        /// <summary>
        /// 展会结束时间
        /// </summary>
        public string EndTime { get; set; }

        /// <summary>
        /// 有无晚宴
        /// </summary>
        public bool IsDinner { get; set; }
        /// <summary>
        /// 申请部门ID
        /// </summary>
        public int DempId { get; set; }
        /// <summary>
        /// 申请人
        /// </summary>
        public string ApplyUser { get; set; }
        /// <summary>
        /// 跟进人
        /// </summary>
        public string FollowPerson { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }
    }
}
