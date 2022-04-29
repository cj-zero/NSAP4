using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Request
{
    /// <summary>
    /// 掉入公海后抢回限制设置
    /// </summary>
    public class GetBackLimitObjectReq
    {
        /// <summary>
        /// 掉入公海后抢回限制
        /// </summary>
        public int BackDay { get; set; }

        /// <summary>
        /// 是否启用掉入公海后抢回限制
        /// </summary>
        public bool BackEnable { get; set; }
    }
}
