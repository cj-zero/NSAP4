using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.Request
{
    public class SalesInvoiceGridDataBindReq : PageReq
    {
        /// <summary>
        /// 默认传T
        /// </summary>
        public string qtype { get; set; }
        public string query { get; set; }
        /// <summary>
        /// 排序字段
        /// </summary>
        public string sortname { get; set; }
        /// <summary>
        /// 排序方式
        /// </summary>
        public string sortorder { get; set; }

    }
}
