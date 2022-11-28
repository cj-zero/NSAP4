using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class QueryPracticalTaskReq : PageReq
    {
        /// <summary>
        /// 技术员Id
        /// </summary>
        public string userId { get; set; }

        /// <summary>
        /// 呼叫主题编码多个逗号隔开
        /// </summary>
        public string solutionCode { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? startTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? endTime { get; set; }
    }
}
