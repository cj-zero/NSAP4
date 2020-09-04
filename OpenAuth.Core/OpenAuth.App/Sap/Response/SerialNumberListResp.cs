using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    public class SerialNumberListResp
    {
        public string ManufSN { get; set; }
        public string InternalSN { get; set; }
        public string Customer { get; set; }
        public string CustmrName { get; set; }
        public int ContractID { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public DateTime DlvryDate { get; set; }
    }
}
