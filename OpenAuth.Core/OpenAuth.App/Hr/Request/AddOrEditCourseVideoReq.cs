using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Hr
{
    /// <summary>
    /// 
    /// </summary>
    public class AddOrEditCourseVideoReq
    {
        /// <summary>
        /// id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 视频名字
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 课程id
        /// </summary>
        public int CourseId { get; set; }
        /// <summary>
        /// 时长
        /// </summary>
        public int Duration { get; set; }
        /// <summary>
        /// 视频地址
        /// </summary>
        public string VideoUrl { get; set; }
    }
}
