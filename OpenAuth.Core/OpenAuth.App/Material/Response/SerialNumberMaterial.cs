using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Response
{
    public class SerialNumberMaterial
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
        /// 关联id
        /// </summary>
        public string QuotationMaterialId { get; set; }
        
    }
}
