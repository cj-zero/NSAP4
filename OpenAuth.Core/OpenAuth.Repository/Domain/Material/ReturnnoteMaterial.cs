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
    public class ReturnnoteMaterial : Entity
    {
        public ReturnnoteMaterial()
        {
            this.MaterialCode = "";
            this.Count = 0;
            this.ReceivingRemark = "";
            this.ShippingRemark = "";
            this.ReturnNoteId = 0;
            this.Check = 0;
            this.MaterialDescription = "";
            this.TotalCount = 0;
            this.GoodQty = 0;
            this.SecondQty = 0;

        }
        /// <summary>
        ///物料编码
        /// </summary>
        [Description("物料编码")]
        public string MaterialCode { get; set; }

        /// <summary>
        ///本次退还数量
        /// </summary>
        [Description("本次退还数量")]
        public int? Count { get; set; }

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
        ///核对验收
        /// </summary>
        [Description("核对验收")]
        public int? Check { get; set; }

        /// <summary>
        ///物料描述
        /// </summary>
        [Description("物料描述")]
        public string MaterialDescription { get; set; }


        /// <summary>
        ///成本价
        /// </summary>
        [Description("成本价")]
        public decimal? CostPrice { get; set; }

        /// <summary>
        ///需退总计
        /// </summary>
        [Description("需退总计")]
        public int? TotalCount { get; set; }

        /// <summary>
        ///良品数量
        /// </summary>
        [Description("良品数量")]
        public int? GoodQty { get; set; }

        /// <summary>
        ///次品数量
        /// </summary>
        [Description("次品数量")]
        public int? SecondQty { get; set; }

        /// <summary>
        ///退料图片
        /// </summary>
        [Description("退料图片")]
        public List<ReturnNoteMaterialPicture> ReturnNoteMaterialPictures { get; set; }

        /// <summary>
        /// 物流Id
        /// </summary>
        [Description("物流Id")]
        public string ExpressId { get; set; }

        /// <summary>
        /// 领料明细Id
        /// </summary>
        [Description("领料明细Id")]
        public string QuotationMaterialId { get; set; }

        /// <summary>
        /// 是否入库（良品） 1已入库
        /// </summary>
        [Description("是否入库（良品） 1已入库")]
        public int? IsGoodFinish { get; set; }

        /// <summary>
        /// 是否入库（次品） 1已入库
        /// </summary>
        [Description("是否入库（次品） 1已入库")]
        public int? IsSecondFinish { get; set; }

        /// <summary>
        /// 折后价格
        /// </summary>
        [Description("折后价格")]
        public decimal DiscountPrices { get; set; }
    }
}