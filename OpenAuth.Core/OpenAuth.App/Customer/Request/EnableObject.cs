using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Request
{
    public class EnableObject
    {
        /// <summary>
        /// 是否启用自动放入公海
        /// </summary>
        public bool? Enable { get; set; }

        /// <summary>
        /// 是否启用自动回收机制
        /// </summary>
        public bool? RecoverEnable { get; set; }

        /// <summary>
        /// 是否启用公海认领分配规则
        /// </summary>
        public bool? ReceiveEnable { get; set; }

        /// <summary>
        /// 是否启用主动掉入公海限制
        /// </summary>
        public bool? AutomaticEnable { get; set; }

        /// <summary>
        /// 是否启用掉入公海后抢回限制
        /// </summary>
        public bool? BackEnable { get; set; }
    }
}
