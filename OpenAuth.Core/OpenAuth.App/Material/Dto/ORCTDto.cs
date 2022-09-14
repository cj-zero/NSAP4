using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Dto
{
    public class ORCTDto
    {
        public string DocType { get; set; }
        public int? DocEntry { get; set; }
        public int? OrderNo { get; set; }
        public decimal? DocTotal { get; set; }
        public DateTime? DocDate { get; set; }
        public string Printed { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentAcct { get; set; }
        public string SettleType { get; set; }
    }

    public class PayMentDto
    {
        public int? DocEntry { get; set; }
        public decimal? DocTotal { get; set; }
        public decimal? PayAmt { get; set; }
        public decimal? DeliAmt { get; set; }
        public decimal? DeliDiscAmt { get; set; }
        public decimal? FeeAmt { get; set; }
        public int? U_BonusSetType { get; set; }
        public decimal? DeliAmtFC { get; set; }
        public decimal? DeliDiscAmtFC { get; set; }
    }
}
