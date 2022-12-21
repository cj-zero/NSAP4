﻿//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a CodeSmith Template.
//
//     DO NOT MODIFY contents of this file. Changes to this
//     file will be lost if the code is regenerated.
//     Author:RenChun Xia
// </autogenerated>
//------------------------------------------------------------------------------
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using OpenAuth.Repository.Core;
using System.Collections.Generic;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 自动冻结表
    /// </summary>
    [Table("payautofreeze")]
    public partial class PayAutoFreeze : Entity
    {
        public PayAutoFreeze()
        {
            this.ModelTypeName = string.Empty;
            this.DataFormat = string.Empty;
            this.CreateUserId = string.Empty;
            this.CreateUserName = string.Empty;
        }

        /// <summary>
        /// 模块类型Id
        /// </summary>
        [Description("模块类型Id")]
        public int? ModelTypeId { get; set; }

        /// <summary>
        /// 模块类型名称
        /// </summary>
        [Description("模块类型名称")]
        public string ModelTypeName { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        [Description("序号")]
        public int? Number { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        [Description("数据")]
        public decimal? DataNumber { get; set; }

        /// <summary>
        /// 数据格式
        /// </summary>
        [Description("数据格式")]
        public string DataFormat { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Description("是否启用")]
        public bool? IsUse { get; set; }

        /// <summary>
        /// 创建人Id
        /// </summary>
        [Description("创建人Id")]
        public string CreateUserId { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Description("创建人")]
        public string CreateUserName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public System.DateTime? CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Description("更新时间")]
        public System.DateTime? UpdateTime { get; set; }
    }
}
