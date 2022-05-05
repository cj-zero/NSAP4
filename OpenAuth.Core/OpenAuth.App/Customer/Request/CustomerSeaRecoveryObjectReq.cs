using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Request
{
    /// <summary>
    /// 公海回收机制设置
    /// </summary>
    public class CustomerSeaRecoveryObjectReq
    {
        /// <summary>
        /// 领取后未报价天数
        /// </summary>
        public int NoPriceDay { get; set; }

        /// <summary>
        /// 领取后未成交天数
        /// </summary>
        public int NoOrderDay { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool RecoverEnable { get; set; }
    }
}
