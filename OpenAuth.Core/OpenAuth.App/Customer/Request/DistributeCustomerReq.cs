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
        public List<Customer> Customers { get; set; }

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
