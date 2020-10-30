using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class ReadMsgReq
    {
        /// <summary>
        /// 用户appid
        /// </summary>
        public int currentUserId { get; set; }
        /// <summary>
        /// 服务单id
        /// </summary>
        public  int serviceOrderId { get; set; }
    }
}
