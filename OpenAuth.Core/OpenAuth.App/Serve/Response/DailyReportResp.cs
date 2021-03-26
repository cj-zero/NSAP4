using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Response
{
    public class DailyReportResp
    {
        public List<string> DailyDates { get; set; }

        public List<ReportResult> ReportResults { get; set; }
    }

    public class ReportResult
    {
        public string DailyDate { get; set; }

        public List<ReportDetail> ReportDetails { get; set; }
    }

    public class ReportDetail
    {
        public DateTime? CreateTime { get; set; }

        public string MaterialCode { get; set; }

        public string ManufacturerSerialNumber { get; set; }

        public List<string> TroubleDescription { get; set; }

        public List<string> ProcessDescription { get; set; }
    }
}
