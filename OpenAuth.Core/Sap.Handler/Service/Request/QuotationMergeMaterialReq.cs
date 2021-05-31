using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sap.Handler.Service.Request
{
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
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }
        /// <summary>
        /// 物料详情
        /// </summary>
        public string MaterialDescription { get; set; }

    }
}
