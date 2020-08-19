using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class GetTechnicianLocationReq
    {
        /// <summary>
        /// 技术员Id
        /// </summary>
        public int TechnicianId { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        public string MaterialType { get; set; }

        /// <summary>
        /// 服务单Id
        /// </summary>
        public int ServiceOrderId { get; set; }
    }
}
