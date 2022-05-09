using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Request
{
    /// <summary>
    /// 分配公海客户请求参数
    /// </summary>
    public class DistributeCustomerReq
    {
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CustomerNo { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// 销售员在ERP3.0的销售代码
        /// </summary>
        public int SlpCode { get; set; }

        /// <summary>
        /// 销售员姓名
        /// </summary>
        public string SlpName { get; set; }

        /// <summary>
        /// 是否分配销售记录
        /// </summary>
        public bool IsSaleHistory { get; set; }
    }
}
