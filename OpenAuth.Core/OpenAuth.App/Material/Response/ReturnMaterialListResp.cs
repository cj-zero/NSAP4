using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Response
{
    public class ReturnMaterialListResp : QuotationMaterial
    {
        /// <summary>
        /// 剩余需退
        /// </summary>
        public int SurplusQty { get; set; }
    }
}
