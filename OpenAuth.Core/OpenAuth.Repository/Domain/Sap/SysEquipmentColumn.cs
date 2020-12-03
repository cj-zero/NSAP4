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
	/// 
	/// </summary>
    public partial class SysEquipmentColumn
    {
        public SysEquipmentColumn()
        {
            this.BaseEntry = 0;
            this.MnfSerial = string.Empty;
            this.ItemCode = string.Empty; 
            this.ItemName = string.Empty; 
            this.BuyUnitMsr = string.Empty; 
            this.OnHand = 0;
            this.WhsCode = string.Empty;
        }
        /// <summary>
        /// 销售单号
        /// </summary>
        [Description("销售单号")]
        public int? BaseEntry { get; set; }
        
        /// <summary>
        /// 序列号
        /// </summary>
        [Description("序列号")]
        public string MnfSerial { get; set; }

        /// <summary>
        /// 设备零件编码
        /// </summary>
        [Description("设备零件编码")]
        public string ItemCode { get; set; }
        /// <summary>
        /// 设备零件介绍
        /// </summary>
        [Description("设备零件介绍")]
        public string ItemName { get; set; }
        /// <summary>
        /// 规格
        /// </summary>
        [Description("规格")]
        public string BuyUnitMsr { get; set; }
        /// <summary>
        /// 库存量
        /// </summary>
        [Description("库存量")]
        public decimal? OnHand { get; set; }
        /// <summary>
        /// 仓库号
        /// </summary>
        [Description("仓库号")]
        public string WhsCode { get; set; }

        /// <summary>
        /// 所用数量
        /// </summary>
        [Description("所用数量")]
        public decimal? Quantity { get; set; }


        /// <summary>
        /// 交货单号
        /// </summary>
        [Description("交货单号")]
        public int? DocEntry { get; set; }

    }
}