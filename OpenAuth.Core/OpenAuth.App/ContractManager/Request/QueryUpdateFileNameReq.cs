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
        /// 客户编码
        /// </summary>
        public string CardCode { get; set; }

        /// <summary>
        /// 后缀名
        /// </summary>
        public string Extension { get; set; }
    }

    public class QueryUpdateNewFileName
    { 
        /// <summary>
        /// 文件新名称
        /// </summary>
        public string FileNewName { get; set; }

        /// <summary>
        /// 文件Id
        /// </summary>
        public string FileId { get; set; }
    }
}
