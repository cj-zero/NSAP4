using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class AttendanceClockPictureReq
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 打卡记录流水Id
        /// </summary>
        public string AttendanceClockId { get; set; }
        /// <summary>
        /// 图片Id
        /// </summary>
        public string PictureId { get; set; }
    }
}
