using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Request
{

    public class StopReq
    {
        public string edge_guid { get; set; }
        public StopControl control { get; set; }
    }
    public class StopControl
    {
        public string cmd_type { get; set; }
        public string arg { get; set; }
    }
    public class StopArg
    {
        /// <summary>
        /// 
        /// </summary>
        public string ip { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ChlInfo> chl { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class ChlInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public int dev_uid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int unit_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int chl_id { get; set; }
    }
}
