﻿//------------------------------------------------------------------------------
//     This code was generated by a CodeSmith Template.
//
//     DO NOT MODIFY contents of this file. Changes to this
//     file will be lost if the code is regenerated.
//     Author:Yubao Li
//------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 物料报价单物料列表
    /// </summary>
    [Table("quotationmaterial")]
    public class QuotationMaterial : Entity
    {
        public QuotationMaterial()
        {
            this.MaterialDescription = "";
            this.Id = "";
            this.Unit = "";
            this.MaterialCode = "";
            this.TotalPrice = 0;
            this.UnitPrice = 0;
            this.Remark = "";
            this.Count = 0;

        }
        /// <summary>
        ///物料描述
        /// </summary>
        [Description("物料描述")]
        public string MaterialDescription { get; set; }
        /// <summary>
        ///单位
        /// </summary>
        [Description("单位")]
        public string Unit { get; set; }

        /// <summary>
        ///物料编码
        /// </summary>
        [Description("物料编码")]
        public string MaterialCode { get; set; }

        /// <summary>
        ///总计
        /// </summary>
        [Description("总计")]
        public decimal? TotalPrice { get; set; }

        /// <summary>
        ///单价
        /// </summary>
        [Description("单价")]
        public decimal? UnitPrice { get; set; }

        /// <summary>
        ///备注
        /// </summary>
        [Description("备注")]
        public string Remark { get; set; }

        /// <summary>
        ///数量
        /// </summary>
        [Description("数量")]
        public int? Count { get; set; }


    }
}