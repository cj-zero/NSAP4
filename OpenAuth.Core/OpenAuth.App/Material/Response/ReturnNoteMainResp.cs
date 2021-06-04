using System;

namespace OpenAuth.App.Material.Response
{
    public class ReturnNoteMainResp
    {
        public int? returnNoteId { get; set; }
        public int? ServiceOrderId { get; set; }

        public int? SalesOrderId { get; set; }

        public int ServiceOrderSapId { get; set; }

        public string CreateUser { get; set; }

        public string CreateTime { get; set; }

        public string UpdateTime { get; set; }

        public decimal TotalMoney { get; set; }

        public string StatusName { get; set; }
        public string Status { get; set; }

        public bool? IsLiquidated { get; set; }

        public string Remark { get; set; }

        public int? InvoiceDocEntry { get; set; }

        public string TerminalCustomer { get; set; }

        public string TerminalCustomerId { get; set; }
        public bool? IsUpDate { get; set; }
    }
}
