using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Request
{
    /// <summary>
    /// 校准绩效明细表
    /// </summary>
    public class QueryCalibrationPerformanceReq:PageReq
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
        /// 校准人
        /// </summary>
        public string UserName { get; set; }
    }
}
