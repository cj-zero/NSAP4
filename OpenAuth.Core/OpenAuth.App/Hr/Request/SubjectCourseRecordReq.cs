using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Hr.Request
{
    public class SubjectCourseRecordReq
    {
        /// <summary>
        /// 专题Id
        /// </summary>
        public int SubjectId { get; set; }

        /// <summary>
        /// 专题课程Id
        /// </summary>
        public int SubjectCourseId { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public int AppUserId { get; set; }

        /// <summary>
        /// 是否完成
        /// </summary>
        public bool IsComplete { get; set; }

    }
}
