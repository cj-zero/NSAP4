using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Request
{
    /// <summary>
    /// 业务员公海认领分配规则设置
    /// </summary>
    public class ClaimRulesObjectReq
    {
        /// <summary>
        /// 每天最多认领数量
        /// </summary>
        public int ReceiveMaxLimit { get; set; }

        /// <summary>
        /// 业务员达到可认领的入职时长最大值
        /// </summary>
        public int ReceiveJobMax { get; set; }

        /// <summary>
        /// 业务员达到可认领的入职时长最小值
        /// </summary>
        public int ReceiveJobMin { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool ReceiveEnable { get; set; }
    }
}
