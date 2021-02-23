using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Response
{
    public class IsServiceRelationsResp
    {
        /// <summary>
        /// 是否通过
        /// </summary>
        public bool ispass { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string message { get; set; }
    }
}
