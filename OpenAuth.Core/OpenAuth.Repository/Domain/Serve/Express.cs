﻿//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a CodeSmith Template.
//
//     DO NOT MODIFY contents of this file. Changes to this
//     file will be lost if the code is regenerated.
//     Author:Yubao Li
// </autogenerated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
	/// 返厂维修物流信息
	/// </summary>
    [Table("express")]
    public partial class Express : Entity
    {
        public Express()
        {
            this.ExpressNumber = string.Empty;
            this.ExpressInformation = string.Empty;
            this.CreateUserId = string.Empty;
            this.Creater = string.Empty;
            this.CreateTime = DateTime.Now;
        }

        public string Id { get; set; }

        /// <summary>
        /// 返厂维修Id
        /// </summary>
        [Description("返厂维修Id")]
        [Browsable(false)]
        public int? ReturnRepairId { get; set; }
        /// <summary>
        /// 快递单号
        /// </summary>
        [Description("快递单号")]
        public string ExpressNumber { get; set; }
        /// <summary>
        /// 快递信息
        /// </summary>
        [Description("快递信息")]
        public string ExpressInformation { get; set; }
        /// <summary>
        /// 发货类型 1客户发货 2仓库发货
        /// </summary>
        [Description("发货类型 1客户发货 2仓库发货")]
        public int? Type { get; set; }
        /// <summary>
        /// 是否收货 1已收货
        /// </summary>
        [Description("是否收货 1已收货")]
        public int? IsCheck { get; set; }

        /// <summary>
        /// 收货时间
        /// </summary>
        [Description("收货时间")]
        public System.DateTime? CheckTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 创建人Id
        /// </summary>
        [Description("创建人Id")]
        [Browsable(false)]
        public string CreateUserId { get; set; }
        /// <summary>
        /// 创建人名称
        /// </summary>
        [Description("创建人名称")]
        public string Creater { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public System.DateTime? CreateTime { get; set; }

        /// <summary>
        /// 物流图片
        /// </summary>
        public virtual List<ExpressPicture> ExpressPictures { get; set; }
    }
}