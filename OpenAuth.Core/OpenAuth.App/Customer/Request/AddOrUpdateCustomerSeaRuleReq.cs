using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Request
{
    public class AddOrUpdateCustomerSeaRuleReq
    {
        /// <summary>
        /// 公海规则Id:为null则表示新增,不为null则进行修改操作
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// 规则名称
        /// </summary>
        public string RuleName { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enable { get; set; }

        public List<Department> Departments { get; set; }

        public List<RuleDetail> RuleDetails { get; set; }
    }

    /// <summary>
    /// 部门信息
    /// </summary>
    public class Department
    {
        /// <summary>
        /// 部门id
        /// </summary>
        public string DepartmentId { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string DepartmentName { get; set; }
    }

    /// <summary>
    /// 规则明细
    /// </summary>
    public class RuleDetail
    {
        /// <summary>
        /// 客户类型
        /// </summary>
        public int CustomerType { get; set; }

        /// <summary>
        /// 天数
        /// </summary>
        public int Day { get; set; }

        /// <summary>
        /// 订单类型
        /// </summary>
        public int OrderType { get; set; }
    }
}
