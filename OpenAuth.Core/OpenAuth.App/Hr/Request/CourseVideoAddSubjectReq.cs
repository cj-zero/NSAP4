using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Hr
{
    /// <summary>
    /// 
    /// </summary>
    public class CourseVideoAddSubjectReq
    {
        /// <summary>
        /// 课程视频id
        /// </summary>
        public int courseVideoId { get; set; }
        /// <summary>
        /// 题目集合
        /// </summary>
        public List<int> subjectIds { get; set; }
    }
}
