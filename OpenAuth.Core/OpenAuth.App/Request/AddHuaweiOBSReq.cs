using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class AddHuaweiOBSReq
    {
        /// <summary>
        /// 前缀
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 文件
        /// </summary>
        public IFormFile File { get; set; }
    }
}
