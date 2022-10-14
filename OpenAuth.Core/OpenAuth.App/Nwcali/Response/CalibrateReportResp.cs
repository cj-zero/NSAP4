using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Response
{
    public class CalibrateReportResp
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string OrgName { get; set; }
        public double? Time { get; set; }
        public int TotalCount { get; set; }
        public int OKCount { get; set; }
        public int NGCount { get; set; }
    }
}
