using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Customer
{
    /// <summary>
    /// 公海规则
    /// </summary>
    [Table("customer_sea_rule")]
    public class CustomerSeaRule : BaseEntity
    {
        /// <summary>
        /// 自增主键Id
        /// </summary>
        public new int Id { get; set; }

        /// <summary>
        /// 规则名称
        /// </summary>
        [Column("Rule_Name")]
        public string RuleName { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Column("Create_User")]
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("Create_Datetime")]
        public DateTime CreateDatetime { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        [Column("Update_User")]
        public string UpdateUser { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [Column("Update_Datetime")]
        public DateTime UpdateDatetime { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        [Column("Is_Delete")]
        public bool IsDelete { get; set; }

        public virtual List<CustomerSeaRuleItem> CustomerSeaRuleItems { get; set; }
    }

    /// <summary>
    /// 公海规则明细
    /// </summary>
    [Table("customer_sea_rule_item")]
    public class CustomerSeaRuleItem : BaseEntity
    {
        /// <summary>
        /// 自增主键
        /// </summary>
        public new int Id { get; set; }

        public virtual CustomerSeaRule CustomerSeaRule { get; set; }

        /// <summary>
        /// 公海规则Id
        /// </summary>
        [Column("Customer_Sea_Rule_Id")]
        public int CustomerSeaRuleId { get; set; }

        /// <summary>
        /// 部门id
        /// </summary>
        [Column("Department_Id")]
        public string DepartmentId { get; set; }

        /// <summary>
        /// 部门主键
        /// </summary>
        [Column("Department_Name")]
        public string DepartmentName { get; set; }

        /// <summary>
        /// 规则类型:0-未报价,1-已成交规则
        /// </summary>
        [Column("Rule_Type")]
        public int RuleType { get; set; }

        /// <summary>
        /// 天数(成交金额)
        /// </summary>
        public string Day { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Column("Create_User")]
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("Create_Datetime")]
        public DateTime CreateDatetime { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        [Column("Update_User")]
        public string UpdateUser { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [Column("Update_Datetime")]
        public DateTime UpdateDatetime { get; set; }
    }
}
