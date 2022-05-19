using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace OpenAuth.App.Customer.Response
{
    public class QueryCustomerSeaRuleResponse
    {
        /// <summary>
        /// 规则id
        /// </summary>
        public int RuleId { get; set; }

        /// <summary>
        /// 规则名称
        /// </summary>
        public string RuleName { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
        public IEnumerable<DepartInfo> DepartInfos { get; set; }

        /// <summary>
        /// 规则明细
        /// </summary>
        public IEnumerable<RuleDetailInfo> RuleDetailInfos { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enable { get; set; }
    }

    public class DepartInfo : IEquatable<DepartInfo>
    {
        /// <summary>
        /// 部门id
        /// </summary>
        public string DepartId { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string DepartName { get; set; }

        public bool Equals([AllowNull] DepartInfo other)
        {
            return this.DepartName == other.DepartName;
        }

        public override int GetHashCode()
        {
            return DepartName.GetHashCode();
        }
    }

    public class RuleDetailInfo : IEquatable<RuleDetailInfo>
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
        /// 交易类型
        /// </summary>
        public int OrderType { get; set; }

        public bool Equals([AllowNull] RuleDetailInfo other)
        {
            return this.CustomerType == other.CustomerType
                && this.Day == other.Day
                && this.OrderType == other.OrderType;
        }

        public override int GetHashCode()
        {
            return this.CustomerType.GetHashCode()
                ^ this.Day.GetHashCode()
                ^ this.OrderType.GetHashCode();
        }
    }
}
