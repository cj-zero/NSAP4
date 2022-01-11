namespace OpenAuth.App.Request
{
    public class QueryBeforeSaleDemandProjectListReq : PageReq
    {
        /// <summary>
        /// 售前需求项目名称
        /// </summary>
        public string KeyWord { get; set; }
        /// <summary>
        /// 状态 1-立项 2-需求 3-开发 4-测试5-实施 6-验收 7-结束
        /// </summary>
        public int? Status { get; set; }
        /// <summary>
        /// 页面类型 0-所有流程
        /// </summary>
        public int PageType { get; set; }
        /// <summary>
        /// 售前需求项目名称
        /// </summary>
        public string ProjectName { get; set; }
        /// <summary>
        /// 售前需求项目编号
        /// </summary>
        public string ProjectNum { get; set; }
        /// <summary>
        /// 发起人Id
        /// </summary>
        public string PromoterId { get; set; }
        /// <summary>
        /// 发起人名字
        /// </summary>
        public string PromoterName { get; set; }
        /// <summary>
        /// 创建时间-开始
        /// </summary>
        public System.DateTime? CreateTimeStart { get; set; }
        /// <summary>
        /// 创建时间-结束
        /// </summary>
        public System.DateTime? CreateTimeEnd { get; set; }
    }
}