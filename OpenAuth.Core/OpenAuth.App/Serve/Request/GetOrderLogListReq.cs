using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class GetOrderLogListReq
    {
        public int ServiceOrderId { get; set; }

        public int? ServiceWorkOrderId { get; set; }
    }
}
