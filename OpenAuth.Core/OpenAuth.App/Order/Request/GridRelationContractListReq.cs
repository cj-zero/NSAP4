using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.Request
{
    public class GridRelationContractListReq: PageReq
    {
        /// <summary>
        /// 排序字段 默认contract_id
        /// </summary>
        public string sortname { get; set; }
        /// <summary>
        /// 排序方式desc
        /// </summary>
        public string sortorder { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 单号
        /// </summary>
        public string DocEntry { get; set; }
    }
}
