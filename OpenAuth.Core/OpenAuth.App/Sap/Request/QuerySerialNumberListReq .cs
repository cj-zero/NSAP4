using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Sap.Request
{
    public class QuerySerialNumberListReq : PageReq
    {
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 制造商序列号
        /// </summary>
        public string ManufSN { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 物料描述
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// 制造商序列号或物料编码
        /// </summary>
        public string ManufSNOrItemCode { get; set; }

    }
}
