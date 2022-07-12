using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Request
{
    public  class CanStopTestReq
    {
        /// <summary>
        /// 生产码
        /// </summary>
        public string GeneratorCode { get; set; }
        /// <summary>
        /// 工步文件地址1
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// 6,7系列必传工步文件地址2
        /// </summary>
        public string FilePath2 { get; set; }
        /// <summary>
        /// 优先启动（1：工步1  2:工步2）
        /// </summary>
        public int FirstStart { get; set; }
        /// <summary>
        /// 系列
        /// </summary>
        public string SeriesName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<StopTest> stopTests { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
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
