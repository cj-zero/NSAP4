using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Response
{
    public class ProductCodeListResp
    {

        public int? SalesOrder { get; set; }
        public int? ProductionOrder { get; set; }
        public string ManufacturerSerialNumber { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialDescription { get; set; }
        public bool IsProtected { get; set; }
        public DateTime? DocDate { get; set; }
    }
}
