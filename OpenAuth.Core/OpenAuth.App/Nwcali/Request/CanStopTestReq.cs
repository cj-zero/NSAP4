using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Request
{
    public  class CanStopTestReq
    {
        public string xmlpath { get; set; }
        public List<StopTest> stopTests { get; set; }
    }
    public class StopTest
    {
        public string GeneratorCode { get; set; }
        public string EdgeGuid { get; set; }
        public string SrvGuid { get; set; }
        public string MidGuid { get; set; }
        public string LowGuid { get; set; }
        public string BtsServerIp { get; set; }
       public int UnitId { get; set; } 
        public int DevUid { get; set; }
    }
}
