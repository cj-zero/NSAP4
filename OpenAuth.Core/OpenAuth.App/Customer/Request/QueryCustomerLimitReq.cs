using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Request
{
    public class QueryCustomerLimitReq: PageReq
    {
        /// <summary>
        /// 分组规则id
        /// </summary>
        public int? Id { get; set; }
    }

}
