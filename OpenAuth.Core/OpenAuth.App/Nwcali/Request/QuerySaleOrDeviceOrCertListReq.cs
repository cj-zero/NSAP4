using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    /// <summary>
    /// 销售订单/设备列表/证书列表查询条件
    /// </summary>
    public class QuerySaleOrDeviceOrCertListReq:PageReq
    {
        /// <summary>
        /// 销售单号
        /// </summary>
        public int? SalesOrderId { get; set; }
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 序列号
        /// </summary>
        public string ManufacturerSerialNumbers { get; set; }
        /// <summary>
        /// 销售订单创建时间开始
        /// </summary>
        public DateTime? SaleStartCreatTime { get; set; }
        /// <summary>
        /// 销售订单创建时间结束
        /// </summary>
        public DateTime? SaleEndCreatTime { get; set; }
        /// <summary>
        /// 规格型号
        /// </summary>
        public string TesterModel { get; set; }
        /// <summary>
        /// 证书编号
        /// </summary>
        public string CertNo { get; set; }
        /// <summary>
        /// 校准人
        /// </summary>
        public string Operator { get; set; }
        /// <summary>
        /// 校准时间开始
        /// </summary>
        public DateTime? StartCalibrationDate { get; set; }
        /// <summary>
        /// 校准时间结束
        /// </summary>
        public DateTime? EndCalibrationDate { get; set; }
    }
}
