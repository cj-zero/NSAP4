using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Response
{
    /// <summary>
    /// 
    /// </summary>
    public class BtsDeviceResp
    {
        /// <summary>
        /// 
        /// </summary>
        public string edge_guid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string srv_guid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string bts_server_version { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string bts_server_ip { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int bts_type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string mid_guid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int dev_uid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string mid_version { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string production_serial { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string low_guid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int low_no { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int unit_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string range_volt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string range_curr_array { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string low_version { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> bts_ids { get; set; }
    }
}
