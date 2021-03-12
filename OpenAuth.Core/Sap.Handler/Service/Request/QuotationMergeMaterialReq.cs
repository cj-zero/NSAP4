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
        
    }
}
