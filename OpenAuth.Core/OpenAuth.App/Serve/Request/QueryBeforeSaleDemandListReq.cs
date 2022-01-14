namespace OpenAuth.App.Request
{
    public class QueryBeforeSaleDemandListReq : PageReq
    {        
        /// <summary>
        /// 项目名称/申请人
        /// </summary>
        public string KeyWord { get; set; }
        /// <summary>
        /// 申请人Id
        /// </summary>
        public string ApplyUserId { get; set; }
        /// <summary>
        /// 申请人
        /// </summary>
        public string ApplyUserName { get; set; }
        /// <summary>
        /// 申请日期-开始
        /// </summary>
        public System.DateTime? ApplyDateStart { get; set; }
        /// <summary>
        /// 申请日期-结束
        /// </summary>
        public System.DateTime? ApplyDateEnd { get; set; }

        /// <summary>
        /// 更新日期-开始
        /// </summary>
        public System.DateTime? UpdateDateStart { get; set; }
        /// <summary>
        /// 更新日期-结束
        /// </summary>
        public System.DateTime? UpdateDateEnd { get; set; }
        /// <summary>
        /// 流程状态0-草稿 1-审批中 2-结束
        /// 数据状态0-草稿 1-销售提交需求 2-销售总助审批 3-需求组提交需求 4-研发总助审批 5-研发确认 6-总经理审批
        /// 7-立项 8-需求提交 9-研发提交10-测试提交11-实施提交12-客户验收(流程结束)13-驳回状态
        /// </summary>
        public int? Status { get; set; }
        /// <summary>
        /// 页面类型 0-所有流程 1-提给我的 2-我处理过
        /// </summary>
        public int PageType { get; set; }
    }
}