using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.Request
{
    public class IsActiveNewReq
    {
    
        public string  DocDate { get; set; }
        public int SboId { get; set; }
        public List<SrialNumbers> srialNumbers { get; set; }
    }
    public class SrialNumbers {

        public string  ItemCode { get; set; }
        public int Count { get; set; }
        public string WhsCod { get; set; }
        public int ItemIndex { get; set; }
    }
}
