using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class ReimburseRevocationReq
    {
        /// <summary>
        /// 报销单id
        /// </summary>
        public int ReimburseInfoId { get; set; }

        /// <summary>
        /// AppId
        /// </summary>
        public int? AppId { get; set; }
    }
}
