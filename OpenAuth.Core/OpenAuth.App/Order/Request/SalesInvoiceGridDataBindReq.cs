using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.Request
{
    public class SalesInvoiceGridDataBindReq : PageReq
    {
        public string qtype { get; set; }
        public string query { get; set; }
        public string sortname { get; set; }
        public string sortorder { get; set; }

    }
}
