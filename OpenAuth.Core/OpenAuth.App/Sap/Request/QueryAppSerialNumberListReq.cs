using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Sap.Request
{
    public class QueryAppSerialNumberListReq : PageReq
    {
        /// <summary>
        /// 服务单Id
        /// </summary>
        public int ServiceOrderId { get; set; }

        /// <summary>
        /// 客户代码
        /// </summary>
        public string CardCode { get; set; }
    }
}
