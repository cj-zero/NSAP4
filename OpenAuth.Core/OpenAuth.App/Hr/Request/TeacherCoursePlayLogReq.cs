using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Hr
{
    /// <summary>
    /// 讲师视频观看记录
    /// </summary>
    public class TeacherCoursePlayLogReq
    {
        /// <summary>
        ///  讲师视频id
        /// </summary>
        public int TeacherCourseId { get; set; }
        /// <summary>
        /// APP用户id
        /// </summary>
        public int AppUserId { get; set; }
        /// <summary>
        /// 观看时长
        /// </summary>
        public int PlayDuration { get; set; }
        /// <summary>
        /// 视频总时长
        /// </summary>
        public int TotalDuration { get; set; }
    }
}
