using System;

namespace OpenAuth.App.Request
{
    public class QueryServiceEvaluateListReq : PageReq
    {
        //todo:添加自己的请求字段
        /// <summary>
        /// 服务单号
        /// </summary>
        public int? ServiceOrderId { get; set; }
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CustomerId { get; set; }
        /// <summary>
        /// 技术员
        /// </summary>
        public string Technician { get; set; }
        /// <summary>
        /// 回访人
        /// </summary>
        public string VisitPeople { get; set; }
        /// <summary>
        /// 开始日期
        /// </summary>
        public DateTime? DateFrom { get; set; }
        /// <summary>
        /// 结束日期
        /// </summary>
        public DateTime? DateTo { get; set; }
    }
}