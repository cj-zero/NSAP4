using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.ModelDto
{
    public class BalDueDto
    {
        public string BalDue { get; set; }
        public string Total { get; set; }
        public List<Balsbodetail> BalSboDetails { get; set; }
        public string Due90 { get; set; }
        public string Total90 { get; set; }
        public BalDueDto()
        {
            BalDue = "0.00";
            Total = "0.00";
            Due90 = "0.00";
            Total90 = "0.00";
            BalSboDetails = new List<Balsbodetail>();
        }
    }

    public class Balsbodetail
    {
        public string id { get; set; }
        public string name { get; set; }
        public string BalSboAmount { get; set; }
    }
}
