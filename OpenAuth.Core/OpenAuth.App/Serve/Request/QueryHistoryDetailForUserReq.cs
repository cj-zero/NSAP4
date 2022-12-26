using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class QueryHistoryDetailForUserReq : PageReq
    {
        /// <summary>
        /// 创建人ID
        /// </summary>
        public string CreateUserId { get; set; }

        /// <summary>
        /// 填报开始日期
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 填报结束日期
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 单据类型 0=全部 1=提成 2=报销 3=结算/代理 4=工资 5=责任
        /// </summary>
        public int? OrderType { get; set; }

        /// <summary>
        /// 支付状态 0=全部 1=待支付 2=已支付
        /// </summary>
        public int? PayStatus { get; set; }
    }

    public class HistoryDetailForUserResp
    {
    
        public int Id { get; set; }//单据ID
        public int Type { get; set; }//类型同上
        public int? ServiceOrderId { get; set; } 
        public decimal? TotalMoney { get; set; }//总金额
        public DateTime? PayTime { get; set; }//支付时间
        public string CustomerId { get; set; }//客户编码
        public string CustomerName { get; set; }//客户名
        public string FlowInstanceId { get; set; }//流程id 不用
        public int? Status { get; set; }//状态 不用
        public string StatusName { get; set; }//状态名
        public int ReimburseId { get; set; }//状态名

        





    }
}
