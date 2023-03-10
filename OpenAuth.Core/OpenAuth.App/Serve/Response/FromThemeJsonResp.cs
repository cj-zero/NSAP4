using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Response
{
    public class FromThemeJsonResp
    {
        /// <summary>
        /// 服务呼叫ID
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 服务呼叫内容
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public string code { get; set; }
    }
}
