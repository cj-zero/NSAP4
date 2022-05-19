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
    /// 黑白名单客户
    /// </summary>
    [Table("special_customer")]
    public class SpecialCustomer : BaseEntity<int>
    {

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
        /// 业务员Id
        /// </summary>
        [Column("Saler_Id")]
        [Description("业务员Id")]
        public string SalerId { get; set; }

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
        public string DepartmentId { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
        [Column("Department_Name")]
        //[Required(ErrorMessage = "部门不能为空")]
        [MaxLength(20)]
        [Description("部门")]
        public string DepartmentName { get; set; }

        /// <summary>
        /// 类型：1白名单，0：黑名单
        /// </summary>
        [Description("类型")]
        public int Type { get; set; }

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
