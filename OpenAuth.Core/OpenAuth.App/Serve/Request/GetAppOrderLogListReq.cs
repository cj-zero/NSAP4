using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class GetAppOrderLogListReq
    {
        /// <summary>
        /// SapId
        /// </summary>
        public int SapOrderId { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        public string MaterialType { get; set; }

        /// <summary>
        /// 日志类型 1客户 2技术员
        /// </summary>
        public int LogType { get; set; }
    }
}
