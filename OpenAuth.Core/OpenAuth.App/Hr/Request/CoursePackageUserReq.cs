using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Hr
{
    /// <summary>
    /// 
    /// </summary>
    public class CoursePackageUserReq
    {
        /// <summary>
        /// 课程包Id
        /// </summary>
        public int CoursePackageId { get; set; }
        /// <summary>
        /// 课程Id
        /// </summary>
        public List<int> Ids { get; set; }
    }
}
