using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class QueryEntrustmentReq : PageReq
    {
        /// <summary>
        /// 委托单号
        /// </summary>
        public int? Id { get; set; }
        /// <summary>
        /// 委托单位
        /// </summary>
        public string EntrustedUnit { get; set; }
        /// <summary>
        /// 销售订单
        /// </summary>
        public int? SaleId { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public int? Status { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
