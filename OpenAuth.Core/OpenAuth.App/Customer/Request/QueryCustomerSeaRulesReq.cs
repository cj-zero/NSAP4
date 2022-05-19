using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Request
{
    public class QueryCustomerSeaRulesReq : PageReq
    {
        /// <summary>
        /// 规则Id,为null则查询全部
        /// </summary>
        public int? Id { get; set; }
    }
}
