using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Request
{
    public class ReceiveCustomerReq
    {
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CustomerNo { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; }

        ///// <summary>
        ///// 销售员代码
        ///// </summary>
        //public int SlpCode { get; set; }

        ///// <summary>
        ///// 销售员名称
        ///// </summary>
        //public string SlpName { get; set; }

        /// <summary>
        /// 是否分配销售历史记录:true-分配,false-不分配
        /// </summary>
        public bool IsSaleHistory { get; set; }
    }
}
