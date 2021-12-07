using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.Request
{
    public class SalesDeliveryListReq:PageReq
    {
        /// <summary>
        /// qtype
        /// </summary>
        public string  qtype { get; set; }
        /// <summary>
        /// Sbo_id:1`a.DocEntry:2`a.CardCode:2`DocStatus:ON`a.Comments:2`c.SlpName:2`GroupNum:13`BeginDate:2021-12-01`EndDate:2021-12-07
        /// </summary>
        public string query { get; set; }
        /// <summary>
        /// 排序字段
        /// </summary>
        public string sortname { get; set; }
        /// <summary>
        /// 排序方式
        /// </summary>
        public string sortorder { get; set; }
    }
}
