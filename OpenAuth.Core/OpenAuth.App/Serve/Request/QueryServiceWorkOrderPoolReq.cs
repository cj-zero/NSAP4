using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class QueryServiceWorkOrderPoolReq : PageReq
    {
        public int? AppUserId { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        public decimal Longitude { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public decimal Latitude { get; set; }
        /// <summary>
        /// 1.30km 2.50km 3.100km 4.200km 5.300km 6.300km以外
        /// </summary>
        public int? Distance { get; set; }
        /// <summary>
        /// 服务方式 1上门服务 2电话服务 3返厂
        /// </summary>
        public int? ServiceMode { get; set; }
    }
}
