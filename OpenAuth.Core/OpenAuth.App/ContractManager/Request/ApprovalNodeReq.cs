using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.ContractManager.Request
{
    public class ApprovalNodeReq
    {
        /// <summary>
        /// 节点名称
        /// </summary>
        public string NodeName { get; set; }

        /// <summary>
        /// 节点用户
        /// </summary>
        public string NodeUser { get; set; }
    }
}
