using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Hr
{
    /// <summary>
    /// 
    /// </summary>
    public class CourseVideoPlayLogReq
    {
        /// <summary>
        /// 
        /// </summary>
        public int CoursePackageId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int CourseId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int CourseVideoId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int AppUserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int PlayDuration { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int TotalDuration { get; set; }
    }
}
