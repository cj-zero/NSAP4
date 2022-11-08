using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class QueryReportReq : PageReq
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
        /// <summary>
        /// 选择范围 -user -org
        /// </summary>
        public string Range { get; set; }
        /// <summary>
        /// 呼叫主题/部门/人员
        /// </summary>
        public List<string> Name { get; set; }

        /// <summary>
        /// 请求类型:1--普通部门,2--生产部门
        /// </summary>
        public int CallType { get; set; }

        public string QueryOrgName { get; set; }

        /// <summary>
        /// 是否研发
        /// </summary>
        public bool IsDevelop { get; set; }
        /// <summary>
        /// 是否售后
        /// </summary>
        public bool IsAfterSale { get; set; }

        /// <summary>
        /// 1 问题描述 2解决方案
        /// </summary>
        public int ViewType { get; set; }
        /// <summary>
        /// 报表名称
        /// </summary>
        public string ReportName { get; set; }
        public string Size { get; set; }
        public string CreateUserId { get; set; }
    }
}
