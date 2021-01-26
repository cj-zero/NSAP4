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
        /// 出库数量
        /// </summary>
        public int? SentQuantity { get; set; }
    }
}
