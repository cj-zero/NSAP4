using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class QueryRerurnRepairListReq : PageReq
    {
        /// <summary>
        /// 服务Id
        /// </summary>
        public string SapId { get; set; }

        /// <summary>
        /// 客户名称/代码
        /// </summary>
        public string Customer { get; set; }

        /// <summary>
        /// 快递单号
        /// </summary>
        public string ExpressNum { get; set; }

        /// <summary>
        /// 寄件人
        /// </summary>
        public string Creater { get; set; }

        /// <summary>
        /// 快递状态 0全部 1未签收  2已签收
        /// </summary>
        public int ExpressState { get; set; }

        /// <summary>
        /// 创建时间（开始）
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// 创建时间（结束）
        /// </summary>
        public string EndDate { get; set; }
    }
}
