using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Workbench.Request
{
    public class PendingReq: PageReq
    {
        /// <summary>
        /// 审批序号
        /// </summary>
        public string ApprovalNumber { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string TerminalCustomer { get; set; }

        /// <summary>
        /// 客户代码
        /// </summary>
        public string TerminalCustomerId { get; set; }

        /// <summary>
        /// 申请人
        /// </summary>
        public string Petitioner { get; set; }

        /// <summary>
        /// 更新时间开始
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 更新时间结束
        /// </summary>
        public DateTime? EndTime { get; set; }
    }
}
