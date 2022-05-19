using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Request
{
    public class BindDeviceReq
    {
        /// <summary>
        /// 生产码
        /// </summary>
        public string GeneratorCode { get; set; }
        /// <summary>
        /// 中位机guid
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// 中位机编码
        /// </summary>
        public int DevUid { get; set; }
        /// <summary>
        /// 边缘计算guid
        /// </summary>
        public string EdgeGuid { get; set; }
        /// <summary>
        /// 上位机guid
        /// </summary>
        public string SrvGuid { get; set; }
        /// <summary>
        /// 绑定类型：1.中位机绑定 2.下位机绑定
        /// </summary>
        public int BindType { get; set; }
        /// <summary>
        /// 上位机ip
        /// </summary>
        public string BtsServerIp { get; set; }
        /// <summary>
        /// 下位机guid
        /// </summary>
        public List<LowList> low_Lists { get; set; }
    }
    /// <summary>
    /// 下位机
    /// </summary>
    public class LowList
    {
        /// <summary>
        /// 单元id
        /// </summary>
        public int UnitId { get; set; }
        /// <summary>
        /// 下位机guid
        /// </summary>
        public string LowGuid { get; set; }
    }
}
