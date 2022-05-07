using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Client.Response
{
    public class GetCustomerCount
    {
        /// <summary>
        /// 总数量
        /// </summary>
        public int Count0 { get; set; }

        /// <summary>
        /// 未报价数量
        /// </summary>
        public int Count1 { get; set; }

        /// <summary>
        /// 已成交数量
        /// </summary>
        public int Count2 { get; set; }

        /// <summary>
        /// 公海领取数量(指在公海中被领取或分配，但是还没做过报价单的客户数量)
        /// </summary>
        public int Count3 { get; set; }

        /// <summary>
        /// 即将掉入公海的客户数量
        /// </summary>
        public int Count4 { get; set; }

    }
}
