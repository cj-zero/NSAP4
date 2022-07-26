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
        public List<string> GeneratorCode { get; set; }
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
        /// <summary>
        /// 启动过滤类型 1:生产码  2:下位机
        /// </summary>

        public int FilterType { get; set; }
        /// <summary>
        /// 下位机列表
        /// </summary>
        public List<LowDeviceList> lowDeviceLists { get; set; }

    }

    /// <summary>
    /// 下位机
    /// </summary>
    public class LowDeviceList
    {
        /// <summary>
        /// 生产码
        /// </summary>
        public string GeneratorCode { get; set; }
        /// <summary>
        /// 边缘计算guid
        /// </summary>
        public string EdgeGuid { get; set; }
        /// <summary>
        /// 上位机guid
        /// </summary>
        public string SrvGuid { get; set; }
        /// <summary>
        /// 中位机guid
        /// </summary>
        public string MidGuid { get; set; }
        /// <summary>
        /// 下位机guid
        /// </summary>
        public string LowGuid { get; set; }
        /// <summary>
        /// 上位机ip
        /// </summary>
        public string BtsServerIp { get; set; }
        /// <summary>
        /// 下位机单元id
        /// </summary>
        public int UnitId { get; set; }
        /// <summary>
        /// 中位机编号
        /// </summary>
        public int DevUid { get; set; }
    }

}
