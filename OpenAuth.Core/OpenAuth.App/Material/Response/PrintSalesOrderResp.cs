using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Response
{
    public class PrintSalesOrderResp
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        public string MaterialCode { get; set; }
        /// <summary>
        /// 设备描述
        /// </summary>
        public string MaterialDescription { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public string Count { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// 单价
        /// </summary>
        public string SalesPrice { get; set; }
        /// <summary>
        /// 总价
        /// </summary>
        public string TotalPrice { get; set; }

        /// <summary>
        /// 库位
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// 销售单号
        /// </summary>
        public string SalesOrder { get; set; }

        /// <summary>
        /// 服务id
        /// </summary>
        public string ServiceOrderSapId { get; set; }
    }
}
