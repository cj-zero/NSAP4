using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    public class RealTimeLocationReportResp
    {
        /// <summary>
        /// 统计类型
        /// </summary>
        public string StatType { get; set; }
        /// <summary>
        /// 统计数据
        /// </summary>
        public List<RealTimeLocationReportSubtableResp> StatList { get; set; }
    }

    public class RealTimeLocationReportSubtableResp
    {

        /// <summary>
        /// 统计键
        /// </summary>
        public string StatId
        {
            get; set;
        }
        /// <summary>
        /// 统计键值对应显示名称
        /// </summary>
        public string StatName { get; set; }
        /// <summary>
        /// 时长
        /// </summary>
        public double Duration { get; set; }
        /// <summary>
        /// 分层数据
        /// </summary>
        public List<RealTimeLocationReportSubtableResp> ReportList { get; set; }
    }

    public class GroupByInfoResp
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string OrgName { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public DateTime CreateTime { get; set; }

    }
}
