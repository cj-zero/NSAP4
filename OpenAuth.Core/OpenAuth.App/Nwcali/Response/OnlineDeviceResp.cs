using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Response
{
    public class OnlineDeviceResp
    {
        public string edge_guid { get; set; }
        public string srv_guid { get; set; }
        public string bts_server_ip { get; set; }
        public ushort? status { get; set; }
        public List<mid_list> mid_Lists { get; set; }
        
    }
    public class mid_list
    {
        public int dev_uid { get; set; }
        public string mid_guid { get; set; }
        public List<low_list> low_Lists { get; set; }
        public ushort? status { get; set; }
        public bool has_test { get; set; }
        public bool has_bind { get; set; }
        public string GeneratorCode { get; set; }
    }
    public class low_list
    {
        public string low_guid { get; set; }
        public int unit_id { get; set; }
        public ushort? status { get; set; }
        public bool has_test { get; set; }
        public bool has_bind { get; set; }
        public string GeneratorCode { get; set; }
        public int? low_no { get; set; }
    }
}
