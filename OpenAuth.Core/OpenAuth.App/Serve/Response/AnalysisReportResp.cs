using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Response
{
    /// <summary>
    /// 分析报表通用
    /// </summary>
    public class AnalysisReportResp
    {
        /// <summary>
        /// 列表名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 列表内容
        /// </summary>
        public List<AnalysisReportSublist> AnalysisReportSublists { get; set; }
    }
    /// <summary>
    /// 子表
    /// </summary>

    public class AnalysisReportSublist
    {
        /// <summary>
        /// 各项名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 个数
        /// </summary>

        public int? Count { get; set; }

        public decimal? TotalMoney { get; set; }
        public string Description { get; set; }
    }
}
