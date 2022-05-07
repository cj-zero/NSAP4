using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Request
{
    public class QueryCustomerSalerListReq : PageReq
    {
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CardCode { get; set; }
    }
}
