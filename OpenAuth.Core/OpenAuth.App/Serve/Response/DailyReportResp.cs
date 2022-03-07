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

    public class DailyReportDetail
    {
        //public string ManufacturerSerialNumber { get; set; }
        //public string MaterialCode { get; set; }
        //public string FromTheme { get; set; }
        //public string TroubleDescription { get; set; }
        //public string ProcessDescription { get; set; } 

        public int ServiceOrderId { get; set; }
        public string CustomerId { get; set; }
        public string Cutomer { get; set; }
        public string Contact { get; set; }
        public string CaontactTel { get; set; }

        public string Technician { get; set; }

        public string ResponseSpeed { get; set; }
        public string SchemeEffectiveness { get; set; }
        public string ServiceAttitude { get; set; }
        public string ProductQuality { get; set; }

        public string ServicePrice { get; set; }
        public string Comment { get; set; }
        public string VisitPeople { get; set; }
        public string CreateTime { get; set; }
        public string CommentDate { get; set; }
    }
}
