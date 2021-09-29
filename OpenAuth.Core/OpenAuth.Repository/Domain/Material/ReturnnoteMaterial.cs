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
    ///退料单详细表
    /// </summary>
    [Table("returnnotematerial")]
    public class ReturnNoteMaterial : Entity
    {
        public ReturnNoteMaterial()
        {
            this.MaterialCode = "";
            this.ReceivingRemark = "";
            this.ShippingRemark = "";
            this.ReturnNoteId = 0;
            this.MaterialDescription = "";

        }
        /// <summary>
        /// 物料类型
        /// </summary>
        public int? MaterialType { get; set; }
        /// <summary>
        /// 行号
        /// </summary>
        public int? LineNum { get; set; }
        /// <summary>
        ///物料编码
        /// </summary>
        [Description("物料编码")]
        public string MaterialCode { get; set; }

        /// <summary>
        ///收货备注
        /// </summary>
        [Description("收货备注")]
        public string ReceivingRemark { get; set; }

        /// <summary>
        ///发货备注
        /// </summary>
        [Description("发货备注")]
        public string ShippingRemark { get; set; }

        /// <summary>
        ///退料单Id
        /// </summary>
        [Description("退料单Id")]
        public int? ReturnNoteId { get; set; }


        /// <summary>
        ///物料描述
        /// </summary>
        [Description("物料描述")]
        public string MaterialDescription { get; set; }


        /// <summary>
        ///良品数量
        /// </summary>
        [Description("良品数量")]
        public bool? IsGood { get; set; }


        /// <summary>
        ///应收发票id
        /// </summary>
        [Description("应收发票id")]
        public int? InvoiceDocEntry { get; set; }

        /// <summary>
        ///良品仓库
        /// </summary>
        [Description("良品仓库")]
        public string GoodWhsCode { get; set; }

        /// <summary>
        ///次品仓库
        /// </summary>
        [Description("次品仓库")]
        public string SecondWhsCode { get; set; }

        /// <summary>
        ///原物料编码
        /// </summary>
        [Description("原物料编码")]
        public string ReplaceMaterialCode { get; set; }

        /// <summary>
        ///原物料描述
        /// </summary>
        [Description("原物料描述")]
        public string ReplaceMaterialDescription { get; set; }

        /// <summary>
        ///SN号和PN号
        /// </summary>
        [Description("SN号和PN号")]
        public string SNandPN { get; set; }
        /// <summary>
        ///原物料SN号和PN号
        /// </summary>
        [Description("原物料SN号和PN号")]
        public string ReplaceSNandPN { get; set; }

        /// <summary>
        ///序列号表id
        /// </summary>
        [Description("序列号表id")]
        public string ReturnNoteProductId { get; set; }

        /// <summary>
        ///金额
        /// </summary>
        [Description("金额")]
        public decimal Money { get; set; }
        /// <summary>
        ///报价单物料id
        /// </summary>
        [Description("报价单物料id")]
        public string QuotationMaterialId { get; set; }
        
        /// <summary>
        ///退料图片
        /// </summary>
        [Description("退料图片")]
        public List<ReturnNoteMaterialPicture> ReturnNoteMaterialPictures { get; set; }
    }
}