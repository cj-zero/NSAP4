using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class TechicianEndRepairReq
    {
        /// <summary>
        /// 服务单Id
        /// </summary>
        public int ServiceOrderId { get; set; }

        /// <summary>
        /// 当前技术员Id
        /// </summary>
        public int CurrentUserId { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        public string MaterialType { get; set; }
    }
}
