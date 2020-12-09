namespace OpenAuth.App.Request
{
    public class QryWfaEshopStatusListReq : PageReq
    {
        //todo:添加自己的请求字段
        /// <summary>
        /// 业务员查询条件
        /// </summary>
        public string QrySalesCode { get; set; }
        /// <summary>
        /// 提交用户查询条件
        /// </summary>
        public string QryUserId { get; set; }

        /// <summary>
        /// 客户编号
        /// </summary>
        public string QryCardCode { get; set; }

        /// <summary>
        /// 客户名称查询条件
        /// </summary>
        public string QryCardName { get; set; }

        /// <summary>
        /// 报价单审批编码
        /// </summary>
        public string QryJobId { get; set; }

        /// <summary>
        /// 报价单编号
        /// </summary>
        public string QryQuotationId { get; set; }
        /// <summary>
        /// 合同编号
        /// </summary>
        public string QryOrderId { get; set; }
    }
}