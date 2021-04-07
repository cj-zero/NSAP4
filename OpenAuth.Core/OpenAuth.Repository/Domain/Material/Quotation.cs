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
using OpenAuth.Repository.Domain.Material;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    ///报价单
    /// </summary>
    [Table("quotation")]
    public class Quotation : BaseEntity<int>
    {
        public Quotation()
        {
            this.Status =0;
            this.InvoiceCompany = "";
            this.ServiceOrderSapId = 0;
            this.SalesOrderId = 0;
            this.TotalMoney = 0;
            this.FlowInstanceId = "";
            this.PayTime = DateTime.Now; ;
            this.CreateUser = "";
            this.CreateTime = DateTime.Now; ;
            this.DeliveryMethod = "";
            this.CreateUserId = "";
            this.ServiceOrderId = 0;
            this.IsProtected = false;
            this.IsDraft = false;
            this.Remark = "";
            this.ShippingAddress = "";
            this.CollectionAddress = "";
            this.QuotationStatus = 0;
            this.ErpOrApp = 1;

        }
        /// <summary>
        ///报价单状态 1 报价单 2 出库单
        /// </summary>
        [Description("报价单状态 1 报价单 2 出库单")]
        public int? Status { get; set; }

        /// <summary>
        ///开票单位 1 新威尔 2 新能源 3 东莞新威
        /// </summary>
        [Description("开票单位 1 新威尔 2 新能源 3 东莞新威")]
        public string InvoiceCompany { get; set; }

        /// <summary>
        ///SAP服务Id
        /// </summary>
        [Description("SAP服务Id")]
        public int ServiceOrderSapId { get; set; }

        /// <summary>
        ///销售单号
        /// </summary>
        [Description("销售单号")]
        public int? SalesOrderId { get; set; }

        /// <summary>
        ///总金额
        /// </summary>
        [Description("总金额")]
        public decimal? TotalMoney { get; set; }

        /// <summary>
        ///工作流程Id
        /// </summary>
        [Description("工作流程Id")]
        public string FlowInstanceId { get; set; }

        /// <summary>
        ///支付时间
        /// </summary>
        [Description("支付时间")]
        public DateTime PayTime { get; set; }

        /// <summary>
        ///创建人名
        /// </summary>
        [Description("创建人名")]
        public string CreateUser { get; set; }

        /// <summary>
        ///创建时间
        /// </summary>
        [Description("创建时间")]
        public DateTime CreateTime { get; set; }

        /// <summary>
        ///发货方式 1 款到发货 2 先票后货
        /// </summary>
        [Description("发货方式 1 款到发货 2 先票后货")]
        public string DeliveryMethod { get; set; }

        /// <summary>
        ///创建人Id
        /// </summary>
        [Description("创建人Id")]
        public string CreateUserId { get; set; }

        /// <summary>
        ///服务单主键Id
        /// </summary>
        [Description("服务单主键Id")]
        public int ServiceOrderId { get; set; }

        /// <summary>
        ///包内保外
        /// </summary>
        [Description("包内保外")]
        public bool? IsProtected { get; set; }

        /// <summary>
        ///是否草稿状态
        /// </summary>
        [Description("是否草稿状态")]
        public bool IsDraft { get; set; }

        /// <summary>
        ///备注
        /// </summary>
        [Description("备注")]
        public string Remark { get; set; }

        /// <summary>
        ///收货地址
        /// </summary>
        [Description("收货地址")]
        public string ShippingAddress { get; set; }

        /// <summary>
        ///收款地址
        /// </summary>
        [Description("收款地址")]
        public string CollectionAddress { get; set; }

        /// <summary>
        ///报价单审批状态
        /// </summary>
        [Description("报价单审批状态")]
        public int? QuotationStatus { get; set; }

        /// <summary>
        ///领料方式
        /// </summary>
        [Description("领料方式")]
        public string AcquisitionWay { get; set; }

        /// <summary>
        ///货币方式
        /// </summary>
        [Description("货币方式")]
        public string MoneyMeans { get; set; }

        /// <summary>
        ///交货日期
        /// </summary>
        [Description("交货日期")]
        public DateTime? DeliveryDate { get; set; }

        /// <summary>
        ///验收期限
        /// </summary>
        [Description("验收期限")]
        public int? AcceptancePeriod { get; set; }
        


        /// <summary>
        ///提交对象1-ERP 2-APP
        /// </summary>
        [Description("提交对象")]
        public int ErpOrApp { get; set; }

        /// <summary>
        ///成本总价
        /// </summary>
        [Description("成本总价")]
        public decimal? TotalCostPrice { get; set; }

        /// <summary>
        ///服务费
        /// </summary>
        [Description("服务费")]
        public decimal? ServiceCharge { get; set; }

        /// <summary>
        ///是否暂定
        /// </summary>
        [Description("是否暂定")]
        public bool? Tentative { get; set; }

        /// <summary>
        ///电子签章流水号
        /// </summary>
        [Description("电子签章流水号")]
        public string PrintNo { get; set; }

        /// <summary>
        ///收货详细地址
        /// </summary>
        [Description("收货详细地址")]
        public string ShippingDA { get; set; }

        /// <summary>
        ///收款详细地址
        /// </summary>
        [Description("收款详细地址")]
        public string CollectionDA { get; set; }

        /// <summary>
        ///税率
        /// </summary>
        [Description("税率")]
        public string TaxRate { get; set; }

        /// <summary>
        ///发票类别
        /// </summary>
        [Description("发票类别")]
        public string InvoiceCategory { get; set; }

        /// <summary>
        ///差旅费
        /// </summary>
        [Description("差旅费")]
        public decimal? TravelExpense { get; set; }

        /// <summary>
        ///预付百分比
        /// </summary>
        [Description("预付百分比")]
        public decimal? Prepay { get; set; }
        /// <summary>
        ///发货前付款百分比
        /// </summary>
        [Description("发货前付款百分比")]
        public decimal? CashBeforeFelivery { get; set; }
        /// <summary>
        ///收货后付款百分比
        /// </summary>
        [Description("收货后付款百分比")]
        public decimal? PayOnReceipt { get; set; }
        /// <summary>
        ///质保后付款百分比
        /// </summary>
        [Description("质保后付款百分比")]
        public decimal? PaymentAfterWarranty { get; set; }
        /// <summary>
        ///领料类型 ture 更换 false 购买
        /// </summary>
        [Description("领料类型")]

        public bool?  IsMaterialType { get; set; }

        /// <summary>
        ///打印次数
        /// </summary>
        [Description("打印次数")]

        public int? PrintTheNumber { get; set; }

        /// <summary>
        ///维修费工时
        /// </summary>
        [Description("维修费工时")]

        public int? ServiceChargeManHour { get; set; }

        /// <summary>
        ///差旅费工时
        /// </summary>
        [Description("差旅费工时")]

        public int? TravelExpenseManHour { get; set; }


        /// <summary>
        /// 物流表
        /// </summary>
        public virtual List<Expressage> Expressages { get; set; }

        /// <summary>
        /// 物料报价单物料列表
        /// </summary>
        public virtual List<QuotationMergeMaterial> QuotationMergeMaterials { get; set; }

        /// <summary>
        /// 报价单设备列表
        /// </summary>
        public virtual List<QuotationProduct> QuotationProducts { get; set; }

        /// <summary>
        /// 报价单操作表
        /// </summary>
        public virtual List<QuotationOperationHistory> QuotationOperationHistorys { get; set; }

        /// <summary>
        /// 报价单图片
        /// </summary>
        public virtual List<QuotationPicture> QuotationPictures { get; set; }

        public override void GenerateDefaultKeyVal()
        {

        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}