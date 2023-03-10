/*
 * @author : Eaven
 * @date : 2022-4-20
 * @desc : 客户规则
 */
using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 客户规则
    /// </summary>
    [Table("customer_limit_Rule")]
    public class CustomerLimitRule : BaseEntity<int>
    {

        /// <summary>
        /// 规则Id
        /// </summary>
        [Description("规则Id")]
        [Column("Customer_limit_Id")]
        public int CustomerLimitId { get; set; }

        public virtual CustomerLimit CustomerLimit { get; set; }

        /// <summary>
        /// 客户类型:0-全部客户,1-未报价客户,2-已成交客户,3-未转销售订单(已报价)客户,4-未交货(已转销售订单)客户
        /// </summary>
        [Description("客户类型")]
        [Column("Customer_Type")]
        public int CustomerType { get; set; }

        /// <summary>
        /// 限制数量
        /// </summary>
        [Description("限制数量")]
        public int Limit { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Column("Create_User")]
        [Required(ErrorMessage = "创建人不能为空")]
        [MaxLength(20)]
        [Description("创建人")]
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("Create_DateTime")]
        [Description("创建时间")]
        public DateTime CreateDatetime { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        [Column("Update_User")]
        [MaxLength(20)]
        [Description("更新人")]
        public string UpdateUser { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("Update_DateTime")]
        [Description("更新时间")]
        public DateTime? UpdateDatetime { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        [Description("是否删除")]
        public bool Isdelete { get; set; }

        public override void GenerateDefaultKeyVal()
        {
        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
