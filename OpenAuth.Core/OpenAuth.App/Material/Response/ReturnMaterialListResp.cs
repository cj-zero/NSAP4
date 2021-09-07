using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Response
{
    public class ReturnMaterialListResp
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
        /// 金额
        /// </summary>
        public decimal Money { get; set; }
        /// <summary>
        /// 关联报价单
        /// </summary>
        public string QuotationMaterialId { get; set; }
        /// <summary>
        /// sn和pn
        /// </summary>
        public string SNandPN { get; set; }
        /// <summary>
        /// 原物料sn和pn
        /// </summary>
        public string ReplaceSNandPN { get; set; }
        /// <summary>
        /// 原物料编码
        /// </summary>
        public string ReplaceMaterialCode { get; set; }
        /// <summary>
        /// 原物料描述
        /// </summary>
        public string ReplaceMaterialDescription { get; set; }
    }
}
