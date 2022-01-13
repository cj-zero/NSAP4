using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Clue.Request
{
    public class AddClueIntentionProductReq
    {
        /// <summary>
        /// 线索ID
        /// </summary>
        public int ClueId { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 物料描述
        /// </summary>
        public string ItemDescription { get; set; }
        /// <summary>
        /// 可用库存
        /// </summary>
        public string AvailableStock { get; set; }
        /// <summary>
        /// 成本单价
        /// </summary>
        public string UnitCost { get; set; }
    }
}
