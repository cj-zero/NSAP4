using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Hr
{
    /// <summary>
    /// 
    /// </summary>
    public class CourseForCoursePackageReq
    {
        /// <summary>
        /// 课程包Id
        /// </summary>
        public int CoursePackageId { get; set; }
        /// <summary>
        /// 课程Id
        /// </summary>
        public List<int> CourseIds { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class CourseSortResp
    {
        /// <summary>
        /// 课程包Id
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int sort { get; set; }
    }
}
