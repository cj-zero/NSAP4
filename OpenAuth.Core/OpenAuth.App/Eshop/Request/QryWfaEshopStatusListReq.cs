namespace OpenAuth.App.Request
{
    public class QryWfaEshopStatusListReq : PageReq
    {
        //todo:添加自己的请求字段
        /// <summary>
        /// 登录手机号
        /// </summary>
        public string QryMobile { get; set; }
        /// <summary>
        /// 登录用户类型 1 客户 2 业务员
        /// </summary>
        public string QryType { get; set; }
        /// <summary>
        /// 订单状态查询 (0 已提交 1 待发货 2 已发货 3 已完成）
        /// </summary>
        public int? QryStatus { get; set; }
  
    }
}