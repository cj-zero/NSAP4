using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Response
{
    public class IncomeSummaryDetailResp
    {
        public int Type { get; set; }
        public List<IncomeSummaryDetail> Detail { get; set; }
    }

    public class IncomeSummaryDetail
    {
        public string Month { get; set; }
        public int? Rank { get; set; }
        public int? Quantity { get; set; }
        public decimal? TotalMoney { get; set; }
        public int? TopQuantity { get; set; }
        public string TopTotalMoney { get; set; }
                
    }
}
