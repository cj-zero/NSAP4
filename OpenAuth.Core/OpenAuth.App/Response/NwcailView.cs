using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    public class NwcailView
    {
        public string CertNo { get; set; }
        public int? No { get; set; }
        public string TesterModel { get; set; }
        public DateTime? CalibrationDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
