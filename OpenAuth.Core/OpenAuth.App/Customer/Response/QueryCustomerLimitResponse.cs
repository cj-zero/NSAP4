using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Response
{
    public class QueryCustomerLimitResponse
    {
        /// <summary>
        /// 分组id
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 规则描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsDelete { get; set; }

        public IEnumerable<RuleResponse> RuleResponses { get; set; }

        public IEnumerable<SalerResponse> SalerResponses { get; set; }
    }

    public class RuleResponse
    {
        /// <summary>
        /// 客户类型
        /// </summary>
        public int CustomerType { get; set; }

        /// <summary>
        /// 数量限制
        /// </summary>
        public int Limit { get; set; }
    }

    public class SalerResponse
    {
        /// <summary>
        /// 销售员用户id
        /// </summary>
        public string SalerId { get; set; }

        /// <summary>
        /// 销售员用户名称
        /// </summary>
        public string SalerName { get; set; }
    }
}
