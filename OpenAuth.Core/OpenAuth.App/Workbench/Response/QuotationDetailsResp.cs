using OpenAuth.App.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Workbench.Response
{
    public class QuotationDetailsResp
    {
        /// <summary>
        ///报价单状态 1 报价单 2 出库单
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        ///开票单位 1 新威尔 2 新能源 3 东莞新威
        /// </summary>
        public string InvoiceCompany { get; set; }

        /// <summary>
        ///销售单号
        /// </summary>
        public int? SalesOrderId { get; set; }

        /// <summary>
        ///总金额
        /// </summary>
        public decimal? TotalMoney { get; set; }

        /// <summary>
        ///支付时间
        /// </summary>
        public DateTime PayTime { get; set; }

        /// <summary>
        ///创建人名
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        ///创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        ///发货方式 1 款到发货 2 先票后货
        /// </summary>
        public string DeliveryMethod { get; set; }

        /// <summary>
        ///创建人Id
        /// </summary>
        public string CreateUserId { get; set; }

        /// <summary>
        ///服务单主键Id
        /// </summary>
        public int ServiceOrderId { get; set; }

        /// <summary>
        ///包内保外
        /// </summary>
        public bool? IsProtected { get; set; }

        /// <summary>
        ///是否草稿状态
        /// </summary>
        public bool IsDraft { get; set; }

        /// <summary>
        ///备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        ///收货地址
        /// </summary>
        public string ShippingAddress { get; set; }

        /// <summary>
        ///收款地址
        /// </summary>
        public string CollectionAddress { get; set; }

        /// <summary>
        ///报价单审批状态
        /// </summary>
        public decimal? QuotationStatus { get; set; }

        /// <summary>
        ///领料方式
        /// </summary>
        public string AcquisitionWay { get; set; }

        /// <summary>
        ///货币方式
        /// </summary>
        public string MoneyMeans { get; set; }

        /// <summary>
        ///交货日期
        /// </summary>
        public DateTime? DeliveryDate { get; set; }

        /// <summary>
        ///验收期限
        /// </summary>
        public int? AcceptancePeriod { get; set; }


        /// <summary>
        ///成本总价
        /// </summary>
        public decimal? TotalCostPrice { get; set; }

        /// <summary>
        ///寄回维修费
        /// </summary>
        public decimal? ServiceChargeJH { get; set; }
        /// <summary>
        ///上门维修费
        /// </summary>
        public decimal? ServiceChargeSM { get; set; }

        /// <summary>
        ///是否暂定
        /// </summary>
        public bool? Tentative { get; set; }

        /// <summary>
        ///电子签章流水号
        /// </summary>
        public string PrintNo { get; set; }

        /// <summary>
        ///收货详细地址
        /// </summary>
        public string ShippingDA { get; set; }

        /// <summary>
        ///收款详细地址
        /// </summary>
        public string CollectionDA { get; set; }

        /// <summary>
        ///税率
        /// </summary>
        public string TaxRate { get; set; }

        /// <summary>
        ///发票类别
        /// </summary>
        public string InvoiceCategory { get; set; }

        /// <summary>
        ///差旅费
        /// </summary>
        public decimal? TravelExpense { get; set; }

        /// <summary>
        ///预付百分比
        /// </summary>
        public decimal? Prepay { get; set; }
        /// <summary>
        ///发货前付款百分比
        /// </summary>
        public decimal? CashBeforeFelivery { get; set; }
        /// <summary>
        ///收货后付款百分比
        /// </summary>
        public decimal? PayOnReceipt { get; set; }
        /// <summary>
        ///质保后付款百分比
        /// </summary>
        public decimal? PaymentAfterWarranty { get; set; }
        /// <summary>
        ///领料类型 1更换 2.销售  3.成本
        /// </summary>

        public int? IsMaterialType { get; set; }

        /// <summary>
        ///寄回维修费工时
        /// </summary>

        public int? ServiceChargeManHourJH { get; set; }

        /// <summary>
        ///上门维修费工时
        /// </summary>

        public int? ServiceChargeManHourSM { get; set; }

        /// <summary>
        ///差旅费工时
        /// </summary>

        public int? TravelExpenseManHour { get; set; }

        /// <summary>
        ///仓库打印状态
        /// </summary>
        public int? PrintWarehouse { get; set; }

        /// <summary>
        ///修改日期
        /// </summary>
        public DateTime? UpDateTime { get; set; }

        /// <summary>
        ///上门维修费成本
        /// </summary>
        public decimal? ServiceChargeSMCost { get; set; }

        /// <summary>
        ///寄回维修费成本
        /// </summary>
        public decimal? ServiceChargeJHCost { get; set; }
        /// <summary>
        ///差旅费成本
        /// </summary>
        public decimal? TravelExpenseCost { get; set; }

        /// <summary>
        /// 物料报价单物料列表
        /// </summary>
        public virtual List<QuotationMergeMaterialResp> QuotationMergeMaterials { get; set; }

        /// <summary>
        /// 报价单设备列表
        /// </summary>
        public virtual List<QuotationProductResp> QuotationProducts { get; set; }

        /// <summary>
        /// 报价单操作表
        /// </summary>
        public virtual List<OperationHistoryResp> QuotationOperationHistorys { get; set; }

        /// <summary>
        /// 报价单图片
        /// </summary>
        public virtual List<FileResp> QuotationPictures { get; set; }
    }

    /// <summary>
    /// 物料报价单物料列表(合并后)
    /// </summary>
    public class QuotationMergeMaterialResp
    {
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
        /// </summary>
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
        ///物料状态 1-更换 2-购买 3-赠送
        /// </summary>
        public int? MaterialType { get; set; }
        /// <summary>
        ///折后价格
        /// </summary>
        public decimal? DiscountPrices { get; set; }
        /// <summary>
        ///仓库号
        /// </summary>
        public string WhsCode { get; set; }
    }

    /// <summary>
    /// 报价单设备列表
    /// </summary>
    public class QuotationProductResp
    {
        /// <summary>
        ///产品编码
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        ///物料编码
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        ///物料描述
        /// </summary>
        public string MaterialDescription { get; set; }

        /// <summary>
        ///保修到期时间
        /// </summary>
        public DateTime? WarrantyExpirationTime { get; set; }

        /// <summary>
        ///保内保外
        /// </summary>
        public bool? IsProtected { get; set; }


        /// <summary>
        /// 物料报价单物料列表
        /// </summary>
        public virtual List<QuotationMaterialResp> QuotationMaterials { get; set; }
    }

    /// <summary>
    /// 物料报价单物料列表
    /// </summary>
    public class QuotationMaterialResp
    {
        /// <summary>
        ///物料描述
        /// </summary>
        public string MaterialDescription { get; set; }
        /// <summary>
        ///单位
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        ///物料编码
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        ///总计
        /// </summary>
        public decimal? TotalPrice { get; set; }

        /// <summary>
        ///单价
        /// </summary>
        public decimal? UnitPrice { get; set; }

        /// <summary>
        ///备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        ///数量
        /// </summary>
        public int? Count { get; set; }

        /// <summary>
        ///最大数量
        /// </summary>
        public decimal? MaxQuantity { get; set; }

        /// <summary>
        ///折扣
        /// </summary>
        public decimal? Discount { get; set; }

        /// <summary>
        ///销售价
        /// </summary>
        public decimal? SalesPrice { get; set; }

        /// <summary>
        ///被替换物料
        /// </summary>
        public string ReplaceMaterialCode { get; set; }

        /// <summary>
        ///是否新物料
        /// </summary>
        public bool NewMaterialCode { get; set; }

        /// <summary>
        ///物料状态 1-更换 2-购买 3-赠送
        /// </summary>
        public int? MaterialType { get; set; }

        /// <summary>
        ///折后价格
        /// </summary>
        public decimal? DiscountPrices { get; set; }

        /// <summary>
        ///仓库号
        /// </summary>
        public string WhsCode { get; set; }

        /// <summary>
        /// 物料附件
        /// </summary>
        public virtual List<FileResp> Files { get; set; }
    }
}
