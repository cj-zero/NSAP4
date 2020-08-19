using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    public class OrderLogListResp
    {
        /// <summary>
        /// 头
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 详细内容
        /// </summary>
        public string Details { get; set; }
        /// <summary>
        /// 时间
        /// </summary>
        public string CreateTime { get; set; }
    }
}
