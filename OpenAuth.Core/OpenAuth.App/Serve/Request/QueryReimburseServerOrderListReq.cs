using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class QueryReimburseServerOrderListReq : PageReq
    {

        /// <summary>
        ///服务单号
        /// </summary>
        public string SapId { get; set; }
        /// <summary>
        ///appid
        /// </summary>
        public string AppId { get; set; }
        //todo:添加自己的请求字段
    }
}
