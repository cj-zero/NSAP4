using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class EditExpressNumReq
    {
        /// <summary>
        /// 当前物流Id
        /// </summary>
        public string ExpressId { get; set; }

        /// <summary>
        /// 物流单号
        /// </summary>
        public string TrackNumber { get; set; }

        /// <summary>
        /// 当前登录者用户Id
        /// </summary>
        public int AppUserId { get; set; }

        /// <summary>
        ///  1领料 2返厂
        /// </summary>
        public int Type { get; set; }
    }
}
