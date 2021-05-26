using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    /// <summary>
    /// 技术员轨迹查询条件
    /// </summary>
    public class QueryTrajectoryReq
    {
        /// <summary>
        /// 技术员
        /// </summary>
        public string Name { get; set; }
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
