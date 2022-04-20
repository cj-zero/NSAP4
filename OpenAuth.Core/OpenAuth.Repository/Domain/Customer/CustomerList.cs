/*
 * @author : Eaven
 * @date : 2022-4-20
 * @desc :  客户黑\白名单
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
    /// 客户黑\白名单
    /// </summary>
    [Table("customer_list")]
    public class CustomerList : BaseEntity<int>
    {

        /// <summary>
        /// 客户Id;客户Id
        /// </summary>
        [Description("客户Id")]
        public int CustomerId { get; set; }

        /// <summary>
        /// 客户编码;客户编码
        /// </summary>
        [Required(ErrorMessage = "客户编码不能为空")]
        [MaxLength(20)]
        [Description("客户编码")]
        public string CustomerNo { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        [Required(ErrorMessage = "客户名称不能为空")]
        [MaxLength(20)]
        [Description("客户名称")]
        public string CustomerName { get; set; }

        /// <summary>
        /// 业务员Id
        /// </summary>
        [Description("业务员Id")]
        public int SalerId { get; set; }

        /// <summary>
        /// 业务员名称
        /// </summary>
        [Required(ErrorMessage = "业务员名称不能为空")]
        [MaxLength(20)]
        [Description("业务员名称")]
        public string SalerName { get; set; }

        /// <summary>
        /// 部门Id
        /// </summary>
        [Description("部门Id")]
        public int DepartmentId { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
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
        /// 备注
        /// </summary>
        [MaxLength(500)]
        [Description("备注")]
        public string Remark { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Required(ErrorMessage = "创建人不能为空")]
        [MaxLength(20)]
        [Description("创建人")]
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public DateTime CreateDatetime { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        [MaxLength(20)]
        [Description("更新人")]
        public string UpdateUser { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
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
