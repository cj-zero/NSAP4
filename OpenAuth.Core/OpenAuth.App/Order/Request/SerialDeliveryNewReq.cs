using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.Request
{
    public class SerialDeliveryNewReq
    {
        public int SboId { get; set; }
        public List<SrialNumbers> srialNumbers { get; set; }
    }
}
