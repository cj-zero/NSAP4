using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.Repository.Domain.Sap
{
    /// <summary>
    /// 出货校准详情
    /// </summary>
    public class ShipmentCalibration
    {
        /// <summary>
        /// 设备型号
        /// </summary>
        public string TesterModel { get; set; }
        /// <summary>
        /// 序列号
        /// </summary>
        public string TesterSn { get; set; }
        /// <summary>
        /// 出货校准完成时间
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// 出货校准操作人
        /// </summary>
        public string Operator { get; set; }
        /// <summary>
        /// 出证人
        /// </summary>
        public string IsSuer { get; set; }
        /// <summary>
        /// 销售单号
        /// </summary>
        public int? SalesOrder { get; set; }

        /// <summary>
        /// 交货号
        /// </summary>
        public int? DeliveryNumber { get; set; }

        /// <summary>
        /// 销售员
        /// </summary>
        public string Salesman { get; set; }
    }
    public class ShipmentCalibration_sql
    {

        /// <summary>
        /// 销售员
        /// </summary>
        public string Salesman { get; set; }
        /// <summary>
        /// 销售单号
        /// </summary>
        public int? SalesOrder { get; set; }
        /// <summary>
        /// 交货号
        /// </summary>
        public int? DeliveryNumber { get; set; }
        /// <summary>
        /// 序列号
        /// </summary>
        public string TesterSn { get; set; }
    }
}
