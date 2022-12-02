using System;
using System.Collections.Generic;

namespace OpenAuth.App.Request
{
    public class QueryoutsourcListReq : PageReq
    {
        public int Id { get; set; }
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
        /// 月份
        /// </summary>
        public int? Month { get; set; }
        /// <summary>
        /// 是否是当前月份
        /// </summary>
        public bool?  IsMonth { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 完工开始时间
        /// </summary>
        public DateTime? CompletionStartTime { get; set; }

        /// <summary>
        /// 完工结束时间
        /// </summary>
        public DateTime? CompletionEndTime { get; set; }
        /// <summary>
        /// 是否修改
        /// </summary>
        public bool? IsUpdate { get; set; }
        /// <summary>
        /// 服务单集合
        /// </summary>
        public List<int?> ServiceOrderIds { get; set; }

        /// <summary>
        /// 费用id
        /// </summary>
        public string OutsourcExpensesId { get; set; }
        //todo:添加自己的请求字段
        public int SelectMode { get; set; }

        /// <summary>
        /// 呼叫主题
        /// </summary>
        public string FromTheme { get; set; }
        public string StatusType { get; set; }

        public string BatchNo { get; set; }
        public string Name { get; set; }
        public int? Status { get; set; }
        public int? ReportId { get; set; }

        public string OrgName { get; set; }
        public decimal MinMoney { get; set; }
        public decimal MaxMoney { get; set; }

    }
}