using System;

namespace OpenAuth.App.Request
{
    public class QueryReimburseInfoListReq : PageReq
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 报销单号
        /// </summary>
        public string MainId { get; set; }

        /// <summary>
        /// 报销状态
        /// </summary>
        public string RemburseStatus { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 报销人
        /// </summary>
        public string CreateUserName { get; set; }

        /// <summary>
        /// 客户
        /// </summary>
        public string TerminalCustomer { get; set; }


        /// <summary>
        /// 服务id
        /// </summary>
        public string ServiceOrderId { get; set; }

        /// <summary>
        /// 报销部门
        /// </summary>
        public string OrgName { get; set; }


        /// <summary>
        /// 费用承担
        /// </summary>
        public string BearToPay { get; set; }


        /// <summary>
        /// 费用承担
        /// </summary>
        public string Responsibility { get; set; }
        

        /// <summary>
        /// 填报开始日期
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 填报结束日期
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 草稿箱
        /// </summary>
        public bool IsDraft { get; set; }


        /// <summary>
        ///报销类别
        /// </summary>
        public string ReimburseType { get; set; }

        /// <summary>
        ///页面类型 1我的提交 2 待处理 3 已处理 4 已驳回 5 未支付 6 已支付
        /// </summary>
        public int? PageType { get; set; }

        /// <summary>
        ///appid
        /// </summary>
        public int? AppId { get; set; }


        /// <summary>
        ///劳务关系
        /// </summary>
        public string ServiceRelations { get; set; }


        /// <summary>
        /// 完工报告开始日期
        /// </summary>
        public DateTime? CompletionStartDate { get; set; }

        /// <summary>
        /// 完工报告出差结束日期
        /// </summary>
        public DateTime? CompletionEndDate { get; set; }


        /// <summary>
        /// 支付日期开始
        /// </summary>
        public DateTime? PaymentStartDate { get; set; }

        /// <summary>
        /// 支付日期结束
        /// </summary>
        public DateTime? PaymentEndDate { get; set; }

        //todo:添加自己的请求字段

        /// <summary>
        /// 创建人ID
        /// </summary>
        public string CreateUserId { get; set; }

        /// <summary>
        /// 日期选择 1.ALL 2.近七日 3.近30日 4.近60日 5.近90日 6.近1年
        /// </summary>
        public int TimeType { get; set; }

        /// <summary>
        /// 单据类型
        /// </summary>
        public int BillType { get; set; }

        /// <summary>
        /// 呼叫主题
        /// </summary>
        public string FromTheme { get; set; }
    }
}