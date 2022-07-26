using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali
{
    /// <summary>
    /// 启动设备列表
    /// </summary>
    public class StartDeviceListResp
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
        /// 上位机ip
        /// </summary>
        public string BtsServerIp { get; set; }
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
        /// 下位机量程
        /// </summary>
        public string RangeCurrArray { get; set; }
        /// <summary>
        /// 中位机编号
        /// </summary>
        public int dev_uid { get; set; }
        /// <summary>
        /// 下位机单元id
        /// </summary>
        public int unit_id { get; set; }
        /// <summary>
        /// 通道id
        /// </summary>
        public int bts_id { get; set; }
    }
}
