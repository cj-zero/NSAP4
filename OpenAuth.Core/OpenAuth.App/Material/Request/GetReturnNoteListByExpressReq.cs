using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    public class GetReturnNoteListByExpressReq : PageReq
    {
        /// <summary>
        /// 退料订单Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 客户名称/客户代码
        /// </summary>
        public string Customer { get; set; }

        /// <summary>
        /// 服务Id
        /// </summary>
        public string SapId { get; set; }

        /// <summary>
        /// 申请人
        /// </summary>
        public string CreaterName { get; set; }

        /// <summary>
        /// 创建日期（开始）
        /// </summary>
        public string BeginDate { get; set; }

        /// <summary>
        /// 创建日期（结束）
        /// </summary>
        public string EndDate { get; set; }

        /// <summary>
        /// 物流单状态  0待收货 1待检验 2待入库
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 物流单号
        /// </summary>
        public string TrackNumber { get; set; }

        /// <summary>
        /// 报价单Id
        /// </summary>
        public string QutationId { get; set; }
    }
}
