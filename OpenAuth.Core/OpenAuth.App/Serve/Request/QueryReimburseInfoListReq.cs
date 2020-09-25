using System;

namespace OpenAuth.App.Request
{
    public class QueryReimburseInfoListReq : PageReq
    {
        /// <summary>
        /// 报销id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 报销状态
        /// </summary>
        public string RemburseStatus { get; set; }


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
        public DateTime? StaticDate { get; set; }

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

        //todo:添加自己的请求字段
    }
}