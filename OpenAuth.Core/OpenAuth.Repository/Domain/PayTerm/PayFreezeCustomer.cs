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
    /// 冻结客户表
    /// </summary>
    [Table("payfreezecustomer")]
    public partial class PayFreezeCustomer : Entity
    {
        public PayFreezeCustomer()
        {
            this.CardCode = string.Empty;
            this.CardName = string.Empty;
            this.SaleId = string.Empty;
            this.SaleName = string.Empty;
            this.FreezeCause = string.Empty;
            this.ListName = string.Empty;
            this.CreateUserId = string.Empty;
            this.CreateUserName = string.Empty;
            this.UpdateUserId = string.Empty;
            this.UpdateUserName = string.Empty;
        }

        /// <summary>
        /// 客户编码
        /// </summary>
        [Description("客户编码")]
        public string CardCode { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        [Description("客户名称")]
        public string CardName { get; set; }

        /// <summary>
        /// 业务员Id
        /// </summary>
        [Description("业务员Id")]
        public string SaleId { get; set; }

        /// <summary>
        /// 业务员
        /// </summary>
        [Description("业务员")]
        public string SaleName { get; set; }

        /// <summary>
        /// 冻结原因
        /// </summary>
        [Description("冻结原因")]
        public string FreezeCause { get; set; }

        /// <summary>
        /// 是否自动解冻
        /// </summary>
        [Description("是否自动解冻")]
        public bool? IsAutoThaw { get; set; }

        /// <summary>
        /// 冻结开始时间
        /// </summary>
        [Description("冻结开始时间")]
        public System.DateTime? FreezeStartTime { get; set; }

        /// <summary>
        /// 冻结结束时间
        /// </summary>
        [Description("冻结结束时间")]
        public System.DateTime? FreezeEndTime { get; set; }

        /// <summary>
        /// 解冻开始时间
        /// </summary>
        [Description("解冻开始时间")]
        public System.DateTime? ThawStartTime { get; set; }

        /// <summary>
        /// 解冻结束时间
        /// </summary>
        [Description("解冻结束时间")]
        public System.DateTime? ThawEndTime { get; set; }

        /// <summary>
        /// 名单类型
        /// </summary>
        [Description("名单类型")]
        public string ListName { get; set; }

        /// <summary>
        /// 冻结类型
        /// </summary>
        [Description("冻结类型")]
        public int? FreezeType { get; set; }

        /// <summary>
        /// 发送次数
        /// </summary>
        [Description("发送次数")]
        public int? SendCount { get; set; }

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
        /// 更新人Id
        /// </summary>
        [Description("更新人Id")]
        public string UpdateUserId { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        [Description("更新人")]
        public string UpdateUserName { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Description("更新时间")]
        public System.DateTime? UpdateTime { get; set; }
    }
}