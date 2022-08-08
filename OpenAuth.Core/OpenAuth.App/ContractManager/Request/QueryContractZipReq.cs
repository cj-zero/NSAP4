using System;
using System.Collections.Generic;

namespace OpenAuth.App.Request
{
    public class QueryContractZipReq
    {
        /// <summary>
        /// 合同申请单Id
        /// </summary>
        public string ContractId { get; set; }

        /// <summary>
        /// 文件Id列表
        /// </summary>
        public List<string> FileIdList { get; set; }

    }
}