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

        /// <summary>
        /// 生产订单
        /// </summary>
        public int? ProductionNo { get; set; }
        /// <summary>
        /// 产品编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 销售订单
        /// </summary>
        public int? SaleOrderNo { get; set; }
        /// <summary>
        /// 销售员
        /// </summary>
        public string SlpName { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }
    }
}