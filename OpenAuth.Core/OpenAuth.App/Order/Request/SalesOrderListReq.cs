using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.Request
{
    public class SalesOrderListReq : PageReq
    {
        public string qtype { get; set; }
        public string query { get; set; }
        public string DocEntry { get; set; }
        public string CardCode { get; set; }
        public string DocStatus { get; set; }
        public string Comments { get; set; }
        public string SlpName { get; set; }
        public string ToCompany { get; set; }
        public string ReceiptStatus { get; set; }
        public string IsContract { get; set; }
        public string sortname { get; set; }
        public string sortorder { get; set; }
    }
}
