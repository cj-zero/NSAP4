using System;

namespace OpenAuth.App.Request
{
    public class QueryCertinfoListReq : PageReq
    {
        /// <summary>
        /// 证书编号
        /// </summary>
        public string CertNo { get; set; }
        /// <summary>
        /// 设备型号
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// 设备出厂编号
        /// </summary>
        public string Sn { get; set; }
        /// <summary>
        /// 资产编号
        /// </summary>
        public string AssetNo { get; set; }
        /// <summary>
        /// 校准人
        /// </summary>
        public string Operator { get; set; }
        /// <summary>
        /// 校准日期开始
        /// </summary>
        public DateTime? CalibrationDateFrom { get; set; }
        /// <summary>
        /// 校准日期结束
        /// </summary>
        public DateTime? CalibrationDateTo { get; set; }
    }
}