/*
 * @author : Eaven
 * @date : 2022-4-20
 * @desc : 业务员最大客户限制
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
    /// 业务员最大客户限制
    /// </summary>
    [Table("customer_limit_saler")]
    public class CustomerLimitSaler : BaseEntity<int>
    {
        /// <summary>
        /// 主表Id
        /// </summary>
        [Description("主表Id")]
        public int CustomerLimitId { get; set; }

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
        [Description("创建人")]
        public string SalerName { get; set; }

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
