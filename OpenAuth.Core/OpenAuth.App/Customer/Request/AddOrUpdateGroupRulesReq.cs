using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Request
{
    public class AddOrUpdateGroupRulesReq
    {
        /// <summary>
        /// 规则id,如果有则进行修改操作,无则进行新增操作
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 批注内容
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 规则列表
        /// </summary>
        public List<Rule> Rules { get; set; }

    }

    public class Rule
    {
        /// <summary>
        /// 客户类型Id
        /// </summary>
        public string CustomerTypeId { get; set; }

        /// <summary>
        /// 客户类型名称
        /// </summary>
        public string CustomerTypeName { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Limit { get; set; }
    }
}
