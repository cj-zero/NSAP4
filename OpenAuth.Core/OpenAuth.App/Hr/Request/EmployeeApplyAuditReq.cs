using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Hr
{
    /// <summary>
    /// 职工加入申请审核
    /// </summary>
    public class EmployeeApplyAuditReq
    {
        /// <summary>
        /// 申请id
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 审核状态(1:未审核 2:审核已通过 3:已驳回 4:封禁)
        /// </summary>
        public int auditState { get; set; }
    }
}
