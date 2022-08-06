using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Hr
{
    /// <summary>
    /// 职工申请查询
    /// </summary>
    public class GetEmployeeApplyListReq
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 审核状态(1:未审核 2:审核已通过 3:已驳回 4.封禁)
        /// </summary>

        public int AuditState { get; set; }

        /// <summary>
        /// 页数
        /// </summary>
        public int pageIndex { get; set; }

        /// <summary>
        ///  每页的数量
        /// </summary>
        public int pageSize { get; set; }


    }
}
