using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class QueryReportReq
    {
        /// <summary>
        /// 年份
        /// </summary>
        public string Year { get; set; }
        /// <summary>
        /// 月份
        /// </summary>
        public string Month { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 部门
        /// </summary>
        public List<string> OrgName { get; set; }
        /// <summary>
        /// 单据类别（1服务单，2E3工程部,3.行政单）
        /// </summary>
        public int VestInOrg { get; set; }
    }
}
