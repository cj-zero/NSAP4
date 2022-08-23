using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Hr
{
    /// <summary>
    /// 
    /// </summary>
    public class TeacherCourseAuditReq
    {
        /// <summary>
        /// 
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 审核状态(1:未审核 2:审核已通过 3:已驳回)
        /// </summary>
        public int auditState { get; set; }
    }
}
