using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class AddOrUpdateServerFlowReq
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
        /// 流程码
        /// </summary>
        public int FlowNum { get; set; }

        /// <summary>
        /// 当前技术员Id
        /// </summary>
        public int AppUserId { get; set; }
    }
}
