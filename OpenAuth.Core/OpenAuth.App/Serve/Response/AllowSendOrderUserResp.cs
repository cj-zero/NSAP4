using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    public class AllowSendOrderUserResp
    {
        /// <summary>
        /// app用户Id
        /// </summary>
        public int? AppUserId { get; set; }
        /// <summary>
        /// NSAP用户Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 已经接的服务单数量
        /// </summary>
        public int? Count { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        public string Name { get; set; }
    }
}
