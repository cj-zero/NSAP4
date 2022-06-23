using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Client.Request
{
    public class MoveInCustomerSeaReq
    {
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CardCode { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string CardName { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}
