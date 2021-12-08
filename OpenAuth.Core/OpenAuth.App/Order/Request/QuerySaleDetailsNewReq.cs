using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.Request
{
    /// <summary>
    /// 交货详情接收类
    /// </summary>
    public class QuerySaleDetailsNewReq
    {
        /// <summary>
        /// 单号
        /// </summary>
        public string DocNum { get; set; }
        /// <summary>
        /// 账套ID
        /// </summary>
        public string SboId { get; set; }
        public string tablename { get; set; }
        public bool ViewCustom { get; set; }
        public bool ViewSales { get; set; }
        public string isCopy { get; set; }
        public string funcID { get; set; }
    }
}
