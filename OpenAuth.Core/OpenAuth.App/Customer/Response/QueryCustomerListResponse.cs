using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Response
{
    public class QueryCustomerListResponse
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
        /// 销售员id
        /// </summary>
        public int SlpCode { get; set; }

        /// <summary>
        /// 销售员名称
        /// </summary>
        public string SlpName { get; set; }

        /// <summary>
        /// 部门id
        /// </summary>
        public string DeptCode { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string DeptName { get; set; }

        /// <summary>
        /// 客户类型:国内客户or国外客户
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 所属行业
        /// </summary>
        public string CompSector { get; set; }
    }
}
