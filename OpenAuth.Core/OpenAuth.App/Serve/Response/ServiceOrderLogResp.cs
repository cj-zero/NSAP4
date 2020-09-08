using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Response
{
    /// <summary>
    /// 
    /// </summary>
    public class ServiceOrderLogResp
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }
        /// <summary>
        /// 操作人
        /// </summary>
        public string CreateUserName { get; set; }
        /// <summary>
        /// 内容
        /// </summary>

        public string Action { get; set; }
    }
}
