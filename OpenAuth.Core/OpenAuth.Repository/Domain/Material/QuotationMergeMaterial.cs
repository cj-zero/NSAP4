
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
    [Table("QuotationMergeMaterial")]
    public partial class QuotationMergeMaterial : Entity
    {
        public QuotationMergeMaterial()
        {
            this.MaterialCode = string.Empty;
            this.MaterialDescription = string.Empty;
            this.Unit = string.Empty;
            this.IsProtected = false;
        }


        /// <summary>
        /// 报价单id
        /// </summary>
        [Description("报价单id")]
        [Browsable(false)]
        public int? QuotationId { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        [Description("物料编码")]
        public string MaterialCode { get; set; }
        /// <summary>
        /// 物料描述
        /// </summary>
        [Description("物料描述")]
        public string MaterialDescription { get; set; }
        /// <summary>
        /// 物料单位
        /// </summary>
        [Description("物料单位")]
        public string Unit { get; set; }
        /// <summary>
        /// 总数量
        /// </summary>
        [Description("总数量")]
        public int? Count { get; set; }
        /// <summary>
        /// 成本价
        /// </summary>
        [Description("成本价")]
        public decimal? CostPrice { get; set; }
        /// <summary>
        /// 销售价
        /// </summary>
        [Description("销售价")]
        public decimal? SalesPrice { get; set; }
        /// <summary>
        /// 总计
        /// </summary>
        [Description("总计")]
        public decimal? TotalPrice { get; set; }
        /// <summary>
        /// 已出库数量
        /// </summary>
        [Description("已出库数量")]
        public int? SentQuantity { get; set; }

        /// <summary>
        ///包内保外
        /// </summary>
        [Description("包内保外")]
        public bool? IsProtected { get; set; }

        /// <summary>
        /// 毛利
        /// </summary>
        [Description("毛利")]
        public decimal? Margin { get; set; }


        /// <summary>
        /// 折扣
        /// </summary>
        [Description("折扣")]
        public decimal? Discount { get; set; }

        /// <summary>
        ///物料状态 1-更换 2-购买 3-赠送
        /// </summary>
        [Description("物料状态")]
        public int? MaterialType { get; set; }

        /// <summary>
        ///折后价格
        /// </summary>
        [Description("折后价格")]
        public decimal? DiscountPrices { get; set; }
    }
}