using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    public partial class SalesOrderMaterialReq : PageReq
    {
        /// <summary>
        /// 销售单号
        /// </summary>
        public string SalesOrderId { get; set; }

        /// <summary>
        /// 客户
        /// </summary>
        public string CustomerCode { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }
        /// <summary>
        /// 业务员
        /// </summary>

        public string SalesMan { get; set; }
        public string ProjectNo { get; set; }
        public string ProduceNo { get; set; }
        /// <summary>
        /// 是否有项目号（ Y 有 N 没有）
        /// </summary>
        public string IsPro { get; set; }
        /// <summary>
        /// 是否有图纸文件（ Y 有 N 没有）
        /// </summary>
        public string IsDraw { get; set; }

    }
}
