using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Request
{
    public class QueryWhiteOrBlackListReq: PageReq
    {
        /// <summary>
        /// 数据类型:1-白名单,0-黑名单
        /// </summary>
        public int Type { get; set; }
    }
}
