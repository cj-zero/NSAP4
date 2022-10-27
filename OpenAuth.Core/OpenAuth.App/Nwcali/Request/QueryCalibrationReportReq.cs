using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Request
{
    /// <summary>
    /// 校准明细报表
    /// </summary>
    public class QueryCalibrationReportReq:PageReq
    {
        /// <summary>
        /// 校准开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 校准开始时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 部门id
        /// </summary>
        public string OrgId { get; set; }
        /// <summary>
        /// 操作人
        /// </summary>
        public string Operator { get; set; }
    }
}
