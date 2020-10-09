using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Response
{
    /// <summary>
    /// 差旅报销单打印
    /// </summary>
    public class PrintReimburseResp
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 出差天数
        /// </summary>
        public int? Day { get; set; }
        /// <summary>
        /// 报销单号
        /// </summary>
        public int? ReimburseId { get; set; }
        /// <summary>
        /// 部门
        /// </summary>
        public string OrgName { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 职位
        /// </summary>
        public string Position { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        public string TerminalCustomer { get; set; }
        /// <summary>
        /// 出差事由
        /// </summary>
        public string FromTheme { get; set; }
        /// <summary>
        /// 飞机补贴
        /// </summary>
        public decimal? Aircraft { get; set; }
        /// <summary>
        /// 火车补贴
        /// </summary>
        public decimal? Train { get; set; }
        /// <summary>
        /// 长途车船补贴
        /// </summary>
        public decimal? Coach { get; set; }
        /// <summary>
        /// 市内交通补贴
        /// </summary>
        public decimal? Transport { get; set; }
        /// <summary>
        /// 住宿补贴
        /// </summary>
        public decimal? PutUp { get; set; }
        /// <summary>
        /// 其他补贴
        /// </summary>
        public decimal? Else { get; set; }
        /// <summary>
        /// 补助补贴
        /// </summary>
        public decimal? Subsidy { get; set; }
        /// <summary>
        /// 总金额
        /// </summary>
        public string Total { get; set; }
    }
}
