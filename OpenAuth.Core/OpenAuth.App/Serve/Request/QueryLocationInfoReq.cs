using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    /// <summary>
    /// 智慧大屏查询条件
    /// </summary>
    public class QueryLocationInfoReq
    {
        /// <summary>
        /// 技术员
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 客户
        /// </summary>
        public string CusName { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 起始时间
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndDate { get; set; }

    }
}
