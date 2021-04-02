using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    [AutoMapTo(typeof(QuotationMergeMaterial))]
    public class QuotationMergeMaterialReq
    {
        /// <summary>
        /// id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 报价单id
        /// </summary>
        public int? QuotationId { get; set; }

        /// <summary>
        /// 退料单Id
        /// </summary>
        public int? ReturnNoteId { get; set; }

        /// <summary>
        /// 出库数量
        /// </summary>
        public int? SentQuantity { get; set; }

        /// <summary>
        /// 入库数量
        /// </summary>
        public int? InventoryQuantity { get; set; }

        /// <summary>
        /// 仓库号
        /// </summary>
        public string WhsCode { get; set; }

        /// <summary>
        ///物料状态 1-更换 2-购买 3-赠送
        /// </summary>
        public string MaterialType { get; set; }

        /// <summary>
        ///折后价格
        /// </summary>
        public decimal? DiscountPrices { get; set; }
    }
}
