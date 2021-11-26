using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.ModelDto
{
    public class FlowChartDto
    {
        /// <summary>
        /// 步骤名称
        /// </summary>
        public string stepName { get; set; }
        /// <summary>
        /// 操作人
        /// </summary>
        public string realAuditorsName { get; set; }
        /// <summary>
        /// 审核人备注
        /// </summary>
        public string realAuditorsComment { get; set; }
        /// <summary>
        /// 操作时间
        /// </summary>
        public string realAuditorsCheckTime { get; set; }
        /// <summary>
        /// 审核时长
        /// </summary>
        public string Audittime { get; set; }
        /// <summary>
        /// 审核结果
        /// </summary>
        public string  realAuditorsResult { get; set; }
    }
}
