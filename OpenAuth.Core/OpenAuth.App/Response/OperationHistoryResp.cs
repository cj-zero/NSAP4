using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    public class OperationHistoryResp
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 审批时常（分钟）
        /// </summary>
        public string IntervalTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUserName { get; set; }
        /// <summary>
        /// 操作内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 审批阶段
        /// </summary>
        public string ApprovalResult { get; set; }

    }
}
