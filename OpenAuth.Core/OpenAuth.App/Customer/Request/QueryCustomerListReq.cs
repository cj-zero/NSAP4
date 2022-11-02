using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Request
{
    public class QueryCustomerListReq : PageReq
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
        /// 销售员
        /// </summary>
        public string SlpName { get; set; }
    }

    /// <summary>
    /// 行业数据
    /// </summary>
    public class cateList
    {
        public string U_CompSector { get; set; }


        public string CardCode { get; set; }
    }
}
