using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Response
{
    public class StartTestResp
    {
        public bool success { get; set; }
        public string error { get; set; }
        public string data { get; set; }
        public string GeneratorCode { get; set; }
        public string EdgeGuid { get; set; }
        public string SrvGuid { get; set; }
        public string BtsServerIp { get; set; }
        public string MidGuid { get; set; }
        public string LowGuid { get; set; }
        public string Department { get; set; }
        public int stepCount { get; set; }
        public int MaxRange { get; set; }
        public string FileIds { get; set; }
        public List<chl_info> chl_info { get; set; }
    }

    public class chl_info
    {
        public int dev_uid { get; set; }
        public int unit_id { get; set; }
        public int chl_id { get; set; }
        public long test_id { get; set; }
        public bool success { get; set; }
        public string error { get; set; }
    }
}
