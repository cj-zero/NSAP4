using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class GetTechnicianServiceMaterialsReq
    {
        /// <summary>
        /// 服务单Id
        /// </summary>
        public int ServiceOrderId { get; set; }

        /// <summary>
        /// 技术员Id
        /// </summary>
        public int TechnicianId { get; set; }
    }
}
