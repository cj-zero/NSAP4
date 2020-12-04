using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Response
{
    /// <summary>
    /// 差旅报销单打印
    /// </summary>
    public class PrintReimburseResp
    {
        /// <summary>
        /// 报销单号
        /// </summary>
        public int? ReimburseId { get; set; }
        /// <summary>
        /// 完工地址
        /// </summary>
        public string CompleteAddress { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 客户代码
        /// </summary>
        public string TerminalCustomerId { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string TerminalCustomer { get; set; }
        /// <summary>
        /// 出差事由
        /// </summary>
        public List<string> FromTheme { get; set; }

        /// <summary>
        /// logo
        /// </summary>
        public string logo { get; set; }

        /// <summary>
        /// 二维码
        /// </summary>
        public string QRcode { get; set; }
        
        /// <summary>
        /// 报销单
        /// </summary>
        public ReimburseInfo Reimburse { get; set; }
        
    }
}
