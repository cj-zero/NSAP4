using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class TechnicianServiceWorkOrderPoolReq : PageReq
    {
        /// <summary>
        /// 经度
        /// </summary>
        public decimal Longitude { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public decimal Latitude { get; set; }

    }
}
