using System;
using System.Collections.Generic;
using System.Text;
using NSAP.Entity.Sales;

namespace OpenAuth.App.Order.Request
{
    public class SerialDeliveryNewReq
    {
        public int SboId { get; set; }
        public List<SrialNumbers> srialNumbers { get; set; }
    }

    public class UpdateDeliveryFlowReq
    {
        /// <summary>
        /// 单号
        /// </summary>
        public int DocEntry { get; set; }
        /// <summary>
        /// 标识
        /// </summary>
        public string Indicator { get; set; }
        /// <summary>
        /// 发票类别
        /// </summary>
        public string U_FPLB { get; set; }
        /// <summary>
        /// 税率
        /// </summary>
        public string U_SL { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// 审核备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 设备箱号
        /// </summary>
        public string CustomFields { get; set; }
        /// <summary>
        /// 行明细
        /// </summary>
        public IList<billSalesDetails> OrderItems { get; set; }
    }

}
