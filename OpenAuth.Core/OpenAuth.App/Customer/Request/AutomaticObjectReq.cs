using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Request
{
    /// <summary>
    /// 主动掉入公海限制
    /// </summary>
    public class AutomaticObjectReq
    {
        /// <summary>
        /// 公海领取后天数限制
        /// </summary>
        public int AutomaticDayLimit { get; set; }

        /// <summary>
        /// 公海领取后次数限制
        /// </summary>
        public int AutomaticLimit { get; set; }

        /// <summary>
        /// 是否启用主动掉入公海限制
        /// </summary>
        public bool AutomaticEnable { get; set; }
    }
}
