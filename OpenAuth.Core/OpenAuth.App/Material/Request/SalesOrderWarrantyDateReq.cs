using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    public partial class SalesOrderWarrantyDateReq : PageReq
    {
        /// <summary>
        /// 销售单号
        /// </summary>
        public int? SalesOrderId { get; set; }

        /// <summary>
        /// 客户
        /// </summary>
        public string Customer { get; set; }

        /// <summary>
        /// 产品编码
        /// </summary>
        public string ManufacturerSerialNumber { get; set; }
        /// <summary>
        /// 销售员
        /// </summary>

        public string SalesMan { get; set; }

    }
}
