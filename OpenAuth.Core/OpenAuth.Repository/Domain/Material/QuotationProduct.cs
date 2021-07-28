﻿//------------------------------------------------------------------------------
//     This code was generated by a CodeSmith Template.
//
//     DO NOT MODIFY contents of this file. Changes to this
//     file will be lost if the code is regenerated.
//     Author:Yubao Li
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    ///  报价单设备列表
    /// </summary>
    [Table("quotationproduct")]
    public class QuotationProduct : Entity
    {
        public QuotationProduct()
        {
            this.ProductCode = string.Empty;
            this.MaterialCode = string.Empty;
            this.MaterialDescription = string.Empty;
            this.IsProtected = false;

        }

        /// <summary>
        ///物料报价单Id
        /// </summary>
        [Description("物料报价单Id")]
        public int? QuotationId { get; set; }

        /// <summary>
        ///产品编码
        /// </summary>
        [Description("产品编码")]
        public string ProductCode { get; set; }

        /// <summary>
        ///物料编码
        /// </summary>
        [Description("物料编码")]
        public string MaterialCode { get; set; }

        /// <summary>
        ///物料描述
        /// </summary>
        [Description("物料描述")]
        public string MaterialDescription { get; set; }

        /// <summary>
        ///保修到期时间
        /// </summary>
        [Description("保修到期时间")]
        public DateTime? WarrantyExpirationTime { get; set; }

        /// <summary>
        ///保内保外
        /// </summary>
        [Description("保内保外")]
        public bool? IsProtected { get; set; }
        /// <summary>
        ///延保时间
        /// </summary>
        [Description("延保时间")]
        public DateTime? WarrantyTime { get; set; }
        
        /// <summary>
        /// 物料报价单物料列表
        /// </summary>
        public virtual List<QuotationMaterial> QuotationMaterials { get; set; }

    }
}