using System;
using System.Collections.Generic;

namespace OpenAuth.App.Request
{
    public class QueryoutsourcListReq : PageReq
    {
        /// <summary>
        /// 结算单id
        /// </summary>
        public string OutsourcId { get; set; }

        /// <summary>
        /// 服务单id
        /// </summary>
        public string ServiceOrderSapId { get; set; }

        /// <summary>
        /// 客户代码or名称
        /// </summary>
        public string Customer { get; set; }

        /// <summary>
        /// 申请人
        /// </summary>
        public string CreateName { get; set; }

        /// <summary>
        /// 创建月份
        /// </summary>
        public DateTime? CreateMonth { get; set; }

        /// <summary>
        /// 服务类型 1上门服务 2.电话服务
        /// </summary>
        public int? ServiceMode { get; set; }

        /// <summary>
        /// 页面类型 1.待处理 2.已处理 3.待支付 4.已支付
        /// </summary>
        public int? PageType { get; set; }

        /// <summary>
        /// 页面状态 1.未审批 2.已审批
        /// </summary>
        //public int? PageStatus { get; set; }

        /// <summary>
        /// 服务单集合
        /// </summary>
        public List<int?> ServiceOrderIds { get; set; }
        //todo:添加自己的请求字段
    }
}