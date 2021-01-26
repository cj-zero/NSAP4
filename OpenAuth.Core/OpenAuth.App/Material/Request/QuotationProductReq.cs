using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    [AutoMapTo(typeof(QuotationProduct))]
    public class QuotationProductReq
    {
        /// <summary>
        ///物料报价单Id
        /// </summary>
        public int? QuotationId { get; set; }

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
        public DateTime WarrantyExpirationTime { get; set; }

        /// <summary>
        ///保内保外
        /// </summary>
        public bool? IsProtected { get; set; }


        /// <summary>
        /// 物料报价单物料列表
        /// </summary>
        public virtual List<QuotationMaterialReq> QuotationMaterials { get; set; }
    }
}
