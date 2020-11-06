using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    public class QueryQuotationListReq : PageReq
    {
        /// <summary>
        /// 状态栏
        /// </summary>
        public int? StartType { get; set; }

        /// <summary>
        /// 领料单号
        /// </summary>
        public int? QuotationId { get; set; }

        /// <summary>
        /// 客户
        /// </summary>
        public string CardCode { get; set; }

        /// <summary>
        /// 服务单SapId
        /// </summary>
        public int? ServiceOrderSapId { get; set; }

        /// <summary>
        /// 服务单Id
        /// </summary>
        public int? ServiceOrderId { get; set; }

        /// <summary>
        /// 申请人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建日期开始
        /// </summary>
        public DateTime? StartCreateTime { get; set; }

        /// <summary>
        /// 创建日期结束
        /// </summary>
        public DateTime? EndCreateTime { get; set; }
    }
}
