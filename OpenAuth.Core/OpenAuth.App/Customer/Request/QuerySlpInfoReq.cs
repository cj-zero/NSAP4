using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Request
{
    public class QuerySlpInfoReq
    {
        /// <summary>
        /// 销售员编号
        /// </summary>
        public int? SlpCode { get; set; }

        /// <summary>
        /// 销售员名称
        /// </summary>
        public string? SlpName { get; set; }
    }
}
