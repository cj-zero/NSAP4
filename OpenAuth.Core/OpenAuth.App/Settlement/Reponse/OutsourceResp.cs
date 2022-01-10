using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Reponse
{
    public class OutsourceResp
    {
        public int Id { get; set; }
        public int? ServiceMode { get; set; }
        public decimal? CostOrgMoney { get; set; }
        public string UpdateTime { get; set; }
        public string CreateTime { get; set; }
        public int? ServiceOrderSapId { get; set; }
        public string TerminalCustomer { get; set; }
        public string TerminalCustomerId { get; set; }
        public string FromTheme { get; set; }
        public string ManufacturerSerialNumber { get; set; }
        public string MaterialCode { get; set; }
        public string StatusName { get; set; }
        public string PayTime { get; set; }
        public decimal? TotalMoney { get; set; }
        public string CreateUser { get; set; }
        public string Remark { get; set; }
        public string IsRejected { get; set; }
    }
}
