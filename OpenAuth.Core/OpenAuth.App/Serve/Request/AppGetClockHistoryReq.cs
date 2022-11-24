using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Request
{
    public class AppGetClockHistoryReq :PageReq
    {
        /// <summary>
        /// 1:全部人  2：指定用户
        /// </summary>
        public int Types { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int AppUserId { get; set; }
        /// <summary>
        /// 打卡日期
        /// </summary>

        public DateTime? ClockDate { get; set; }
        /// <summary>
        /// 打卡类型（0：未知  1：签到  2：签退）
        /// </summary>
        public int ClockType { get; set; }
    }
}
