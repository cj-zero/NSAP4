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
        public int? SalesOrderId { get; set; }

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
        public int? ReviewCode
        {
            get
            {
                return 0;
            }
            set { ReviewCode = value; }
        }


    }
}
