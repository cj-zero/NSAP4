using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    public class QueryCommissionOrderReq : PageReq
    {
        public QueryCommissionOrderReq()
        {
            this.PageType = 1;
        }
        /// <summary>
        /// 提成单号
        /// </summary>
        public int? Id { get; set; }
        /// <summary>
        /// 申请人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 服务单SapId
        /// </summary>
        public int? ServiceOrderSapId { get; set; }
        /// <summary>
        /// 销售单号
        /// </summary>
        public int? SalesOrderId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? AppUserId { get; set; }
        /// <summary>
        /// 客户代码/名称
        /// </summary>
        public string TerminalCustomer { get; set; }

        /// <summary>
        /// 1.待处理 2.已处理 || PageType == 1我的提成界面，PageType == 2审批中报表下提成单
        /// </summary>
        public int? PageType { get; set; }
        /// <summary>
        /// 批次号
        /// </summary>
        public string BatchNo { get; set; }
        /// <summary>
        /// 部门
        /// </summary>
        public string Org { get; set; }
        /// <summary>
        /// 报表ID
        /// </summary>
        public int? CommissionReportId { get; set; }

        /// <summary>
        /// 创建日期开始
        /// </summary>
        public DateTime? StartCreateTime { get; set; }

        /// <summary>
        /// 创建日期结束
        /// </summary>
        public DateTime? EndCreateTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? Status { get; set; }
    }

    public class AddReportReq
    {
        /// <summary>
        /// 
        /// </summary>
        public int? AppUserId { get; set; }

        public List<int> Ids { get; set; }
    }

    public class BatchAccraditationReq
    {
        public List<int> Ids { get; set; }
        public DateTime? PayTime { get; set; }
    }
}
