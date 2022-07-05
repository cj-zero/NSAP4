using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Response
{
    public class ContractReviewResp
    {
        public int? DocNum { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public string ItemCode { get; set; }
        public string Dscription { get; set; }
        public int? SlpCode { get; set; }
        public DateTime? ReviewSubmitTime { get; set; }
        public string ContractReviewCode { get; set; }
    }
}
