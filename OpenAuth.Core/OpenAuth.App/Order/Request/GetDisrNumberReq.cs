using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.Request
{
    public class GetDisrNumberReq:PageReq
    {
        public string  ExistsSerialStr { get; set; }
        public string ItemCode { get; set; }
        public string SboId { get; set; }
        public string Status { get; set; }
        public string WhsCode { get; set; }
        public string serial { get; set; }
        public string qtype { get; set; }
        public string sortname { get; set; }
        public string sortorder { get; set; }
    }
}
