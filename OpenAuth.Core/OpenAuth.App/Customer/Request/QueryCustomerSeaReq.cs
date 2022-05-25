using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Request
{
    public class QueryCustomerSeaReq : PageReq
    {
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CardCode { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string CardName { get; set; }

        /// <summary>
        /// 创建开始时间
        /// </summary>
        public DateTime? CreateStartTime { get; set; }

        /// <summary>
        /// 创建结束时间
        /// </summary>
        public DateTime? CreateEndTime { get; set; }

        /// <summary>
        /// 掉入公海开始时间
        /// </summary>
        public DateTime? FallIntoStartTime { get; set; }

        /// <summary>
        /// 掉入公海结束时间
        /// </summary>
        public DateTime? FallIntoEndTime { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string DepartMent { get; set; }
    }
}
