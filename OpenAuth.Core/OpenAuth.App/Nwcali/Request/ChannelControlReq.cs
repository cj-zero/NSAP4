using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Request
{
    /// <summary>
    /// 
    /// </summary>
    public  class ChannelControlReq
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
        /// 启动类型（6，7系列特有） 1：相邻对接启动  2：对称对接启动
        /// </summary>
        public int? TestType { get; set; }

    }
}
