﻿using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    [AutoMapTo(typeof(QuotationMaterial))]
    public class QuotationMaterialReq
    {
        /// <summary>
        ///物料报价单Id
        /// </summary>
        public int QuotationId { get; set; }

        /// <summary>
        ///报价单设备Id
        /// </summary>
        public string QuotationProductId { get; set; }

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
        ///仓库数量
        /// </summary>
        public int? WarehouseQuantity { get; set; }

    }
}