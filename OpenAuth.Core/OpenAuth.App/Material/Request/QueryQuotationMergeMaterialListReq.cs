namespace OpenAuth.App.Request
{
    public class QueryQuotationMergeMaterialListReq : PageReq
    {
        /// <summary>
        /// 报价单id
        /// </summary>
        public int? QuotationId { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }
        /// <summary>
        /// 物料描述
        /// </summary>
        public string MaterialDescription { get; set; }
        /// <summary>
        /// 物料单位
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 总数量
        /// </summary>
        public int? Count { get; set; }
        /// <summary>
        /// 成本价
        /// </summary>
        public decimal? CostPrice { get; set; }
        /// <summary>
        /// 销售价
        /// </summary>
        public decimal? SalesPrice { get; set; }
        /// <summary>
        /// 总计
        /// </summary>
        public decimal? TotalPrice { get; set; }
        /// <summary>
        /// 已出库数量
        /// </summary>x
        public int? SentQuantity { get; set; }

        /// <summary>
        ///包内保外
        /// </summary>
        public bool? IsProtected { get; set; }

        /// <summary>
        /// 毛利
        /// </summary>
        public decimal? Margin { get; set; }


        /// <summary>
        /// 折扣
        /// </summary>
        public decimal? Discount { get; set; }

        /// <summary>
        ///物料状态 1-更换 2-购买 3-领料
        /// </summary>
        public int MaterialType { get; set; }

        /// <summary>
        ///折后价格
        /// </summary>
        public decimal? DiscountPrices { get; set; }

        /// <summary>
        ///仓库号
        /// </summary>
        public string WhsCode { get; set; }
        //todo:添加自己的请求字段
    }
}