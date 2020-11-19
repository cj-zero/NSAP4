using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class AddReturnRepairReq
    {
        /// <summary>
        /// 服务单Id
        /// </summary>
        public int ServiceOrderId { get; set; }

        /// <summary>
        /// SapId
        /// </summary>
        public int ServiceOrderSapId { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        public string MaterialType { get; set; }

        /// <summary>
        /// 当前登录者AppId
        /// </summary>
        public int AppUserId { get; set; }

        /// <summary>
        /// 物流单号集合
        /// </summary>
        public Dictionary<string,string> TrackInfos { get; set; }
    }
}
