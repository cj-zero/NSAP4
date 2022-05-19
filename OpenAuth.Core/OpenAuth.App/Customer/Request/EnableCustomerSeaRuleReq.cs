using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Request
{
    public class EnableCustomerSeaRuleReq
    {
        /// <summary>
        /// 规则id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enable { get; set; }
    }
}
