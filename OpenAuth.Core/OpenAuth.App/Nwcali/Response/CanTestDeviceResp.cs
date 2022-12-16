using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Response
{
 
    public class DeviceTestResponse
    {
        public string GeneratorCode { get; set; }
        public string EdgeGuid { get; set; }
        public string SrvGuid { get; set; }
        public string BtsServerIp { get; set; }
        public string MidGuid { get; set; }
        public string LowGuid { get; set; }
        public string Department { get; set; }
        public int stepCount { get; set; }
        public int MaxRange { get; set; }
        public CanTestDeviceResp canTestDeviceResp { get; set; }
    }

    public class CanTestDeviceResp
    {
        public string edge_guid { get; set; }
        public control control { get; set; }
    }
    public class control
    {
        public string cmd_type { get; set; }
        public string arg { get; set; }
    }
    public class arg
    {
        public string srv_guid { get; set; }
        public int dev_uid { get; set; }
        public string ip { get; set; }
        public string step_data { get; set; }
        public string file_id { get; set; }
        public string step_file_name { get; set; }
        public string batch_no { get; set; }
        public string test_name { get; set; }
        public string creator { get; set; }
        public int start_step { get; set; }
        public int scale { get; set; }
        public int battery_mass { get; set; }
        public string desc { get; set; }
        public string barcode { get; set; }
        public List<chl> chl { get; set; }
    }
    public class chl
    {
        public int dev_uid { get; set; }
        public int unit_id { get; set; }
        public int chl_id { get; set; }
        /// <summary>
        /// 质量(整数,单位:微克)
        /// </summary>
        public int battery_mass { get; set; }
        public string barcode { get; set; }
        /// <summary>
        /// 备注,如果为空则会使用工步文件中的信息
        /// </summary>
        public string desc { get; set; }
        public string xwj_guid { get; set; }
    }
}
