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
    [Table("customer_limit")]
    public class CustomerLimit : BaseEntity<int>
    {
        /// <summary>
        /// 规则名称
        /// </summary>
        [Required(ErrorMessage = "规则名称不能为空")]
        [MaxLength(100)]
        [Description("规则名称")]
        public string Name { get; set; }

        /// <summary>
        /// 规则描述
        /// </summary>
        [MaxLength(100)]
        [Description("规则描述")]
        public string Describe { get; set; }

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
