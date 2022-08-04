using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Hr
{
    /// <summary>
    /// 开课申请
    /// </summary>
    public class TeacherCourseApplyReq
    {
        /// <summary>
        /// 课程标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 适合人员
        /// </summary>
        public string ForTheCrowd { get; set; }
        /// <summary>
        /// 讲课方式
        /// </summary>
        public int TeachingMethod { get; set; }
        /// <summary>
        /// 讲课地址
        /// </summary>
        public string TeachingAddres { get; set; }
        /// <summary>
        /// 讲课人
        /// </summary>
        public int AppUserId { get; set; }
        /// <summary>
        /// 课程封面图
        /// </summary>
        public string BackgroundImage { get; set; }
    }
}
