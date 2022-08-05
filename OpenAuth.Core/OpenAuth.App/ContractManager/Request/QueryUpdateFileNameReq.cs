using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.ContractManager.Request
{
    public class QueryUpdateFileNameReq
    {
        /// <summary>
        /// 文件Id
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// 文件扩展名
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// 新名称
        /// </summary>
        public string NewName { get; set; }

    }
}
