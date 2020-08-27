using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class SaveWorkOrderSolutionReq
    {
        /// <summary>
        /// 服务单Id
        /// </summary>
        public int ServiceOrderId { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        public string MaterialType { get; set; }

        /// <summary>
        /// 当前技术员App用户Id
        /// </summary>
        public int CurrentUserId { get; set; }

        /// <summary>
        /// 解决方案Id
        /// </summary>
        public string SolutionId { get; set; }
    }
}
