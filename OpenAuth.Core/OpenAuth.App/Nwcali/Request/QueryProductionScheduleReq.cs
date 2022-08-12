using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class QueryProductionScheduleReq : PageReq
    {
        public int? DocEntry { get; set; }
        public string GeneratorCode { get; set; }
        public string ProductionStatus { get; set; }
        public string DeviceStatus { get; set; }
        public string NwcailStatus { get; set; }
        public string ReceiveStatus { get; set; }
    }
}
