using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.Request
{
    public class GridCopyItemListReq : PageReq
    {
        /// <summary>
        /// 默认传"T"
        /// </summary>
        public string qtype { get; set; }
        /// <summary>
        /// 排序字段 默认传"a.docentry"
        /// </summary>
        public string sortname { get; set; }
        /// <summary>
        /// 排序方式 默认传"desc"
        /// </summary>
        public string sortorder { get; set; }
        /// <summary>
        /// 默认传"sales"
        /// </summary>
        public string doctype { get; set; }
        /// <summary>
        /// 单据类型 默认不传和23是销售报价单 ，17销售订单，15销售交货，16销售退货，13应收发票，14应收贷项凭证，wtr1库转储存
        /// </summary>
        public string  txtCopyDocType { get; set; }
        /// <summary>
        /// 客户
        /// </summary>
        public string  txtCardCode { get; set; }
        /// <summary>
        /// 物料
        /// </summary>
        public string  txtItemCode { get; set; }
        /// <summary>
        /// 单号
        /// </summary>
        public string  txtDocEntry { get; set; }

    }
}
