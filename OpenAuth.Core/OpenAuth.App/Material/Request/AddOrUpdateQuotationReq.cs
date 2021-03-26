using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    /// <summary>
    /// 报价单
    /// </summary>
    [Table("Quotation")]
    [AutoMapTo(typeof(Quotation))]
    public partial class AddOrUpdateQuotationReq
    {
        /// <summary>
        ///AppId
        /// </summary>
        public int? AppId { get; set; }
        /// <summary>
        ///报价单Id
        /// </summary>
        public int? Id { get; set; }
        /// <summary>
        ///报价单状态 1 报价单 2 出库单
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        ///开票单位 1 新威尔 2 新能源 3 东莞新威
        /// </summary>
        public string InvoiceCompany { get; set; }

        /// <summary>
        ///SAP服务Id
        /// </summary>
        public int ServiceOrderSapId { get; set; }

        /// <summary>
        ///销售单号
        /// </summary>
        public int? SalesOrderId { get; set; }

        /// <summary>
        ///总金额
        /// </summary>
        public decimal? TotalMoney { get; set; }

        /// <summary>
        ///工作流程Id
        /// </summary>
        public string FlowInstanceId { get; set; }

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
        ///报价单审批状态
        /// </summary>
        public int? QuotationStatus { get; set; }

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
        ///服务费
        /// </summary>
        public decimal? ServiceCharge { get; set; }

        /// <summary>
        ///删除报价单
        /// </summary>
        public List<int> QuotationIds { get; set; }
        /// <summary>
        ///收货详细地址
        /// </summary>
        public string ShippingDA { get; set; }

        /// <summary>
        ///收款详细地址
        /// </summary>
        public string CollectionDA { get; set; }

        /// <summary>
        ///是否暂定
        /// </summary>
        public bool? Tentative { get; set; }

        /// <summary>
        ///提交对象1-ERP 2-APP
        /// </summary>
        public int ErpOrApp { get; set; }

        /// <summary>
        ///成本总价
        /// </summary>
        public decimal? TotalCostPrice { get; set; }

        /// <summary>
        ///部门名称
        /// </summary>
        public string OrgName { get; set; }

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
        /// 报价单设备列表
        /// </summary>
        public virtual List<QuotationProductReq> QuotationProducts { get; set; }

        /// <summary>
        /// 报价单零件合并列表
        /// </summary>
        public virtual List<QuotationMergeMaterialReq> QuotationMergeMaterialReqs { get; set; }

        /// <summary>
        /// 物流表
        /// </summary>
        public virtual ExpressageReq ExpressageReqs { get; set; }

        /// <summary>
        /// 操作详情表
        /// </summary>
        public virtual List<QuotationOperationHistory> QuotationOperationHistorys { get; set; }
        

    }
}
