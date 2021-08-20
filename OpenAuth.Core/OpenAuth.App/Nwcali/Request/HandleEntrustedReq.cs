using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class HandleEntrustedReq
    {
        /// <summary>
        /// 委托单ID
        /// </summary>
        public int? Id { get; set; }
        /// <summary>
        /// 委托单详细ID
        /// </summary>
        public string EntrustmentId { get; set; }
        /// <summary>
        /// 承检方收件人ID
        /// </summary>
        public string ReceiptUserId { get; set; }
        /// <summary>
        /// 承检方收件人
        /// </summary>
        public string ReceiptUser { get; set; }
        /// <summary>
        /// 是否更新状态
        /// </summary>
        public bool IsUpdateStatus { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 操作类型 1-更新全部 2-再次查询 3-超出校准范围
        /// </summary>
        public string Type { get; set; }
    }
}
