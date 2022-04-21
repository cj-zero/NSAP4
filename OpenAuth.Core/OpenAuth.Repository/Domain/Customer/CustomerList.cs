/*
 * @author : Eaven
 * @date : 2022-4-20
 * @desc :  客户
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
    /// 客户
    /// </summary>
    [Table("customer_list")]
    public class CustomerList : BaseEntity<int>
    {

        /// <summary>
        /// 客户Id;客户Id
        /// </summary>
        [Column("Customer_Id")]
        [Description("客户Id")]
        public int CustomerId { get; set; }

        /// <summary>
        /// 客户编码;客户编码
        /// </summary>
        [Column("Customer_No")]
        [Required(ErrorMessage = "客户编码不能为空")]
        [MaxLength(20)]
        [Description("客户编码")]
        public string CustomerNo { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        [Column("Customer_Name")]
        [Required(ErrorMessage = "客户名称不能为空")]
        [MaxLength(20)]
        [Description("客户名称")]
        public string CustomerName { get; set; }

        /// <summary>
        /// 客户来源;客户来源
        /// </summary>
        [Column("Customer_Source")]
        [MaxLength(200)]
        public string CustomerSource { get; set; }

        /// <summary>
        /// 业务员Id
        /// </summary>
        [Column("Saler_Id")]
        [Description("业务员Id")]
        public int SalerId { get; set; }

        /// <summary>
        /// 业务员名称
        /// </summary>
        [Column("Saler_Name")]
        [Required(ErrorMessage = "业务员名称不能为空")]
        [MaxLength(20)]
        [Description("业务员名称")]
        public string SalerName { get; set; }

        /// <summary>
        /// 部门Id
        /// </summary>
        [Column("Department_Id")]
        [Description("部门Id")]
        public int DepartmentId { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
        [Column("Department_Name")]
        [Required(ErrorMessage = "部门不能为空")]
        [MaxLength(20)]
        [Description("部门")]
        public string DepartmentName { get; set; }

        /// <summary>
        /// 类型：1白名单，0：黑名单
        /// </summary>
        [Description("类型")]
        public int Type { get; set; }

        /// <summary>
        /// 标签Id;1：未报价，2：已成交，3：公海领取，4：即将掉入公海
        /// </summary>
        [Column("Labe_Index")]
        [Description("标签Id")]
        public int LabeIndex { get; set; }

        /// <summary>
        /// 标签;未报价、已成交、公海领取、即将掉入公海
        /// </summary>
        [Description("标签")]
        public string Labe { get; set; }

        /// <summary>
        /// 订单类型;1：未报价，2：已成交
        /// </summary>
        [Column("Order_Type")]
        [Description("订单类型")]
        public int OrderType { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [MaxLength(500)]
        [Description("备注")]
        public string Remark { get; set; }

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
