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
        /// 状态
        /// </summary>
        public int? Status { get; set; }
        /// <summary>
        /// 页面类型 0-所有流程 1-提给我的 2-我处理过
        /// </summary>
        public int PageType { get; set; }
    }
}