using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using OpenAuth.Repository.Core;
using OpenAuth.Repository.Domain.Material;

namespace OpenAuth.Repository.Domain.Material
{
    /// <summary>
    /// 物料更换记录
    /// </summary>
    [Table("materialreplacerecord")]
    public class MaterialReplaceRecord : Entity
    {
        public MaterialReplaceRecord()
        {
            this.ProductCode = string.Empty;
            this.ReplaceMaterialCode = string.Empty;
            this.ReplaceMaterialDescription = string.Empty;
            this.ReplaceSNandPN = string.Empty;
            this.SNandPN = string.Empty;
        }

        /// <summary>
        ///物料报价单Id
        /// </summary>
        [Description("物料报价单Id")]
        public int QuotationId { get; set; }
        /// <summary>
        ///序列号
        /// </summary>
        [Description("序列号")]
        public string ProductCode { get; set; }
        /// <summary>
        ///已领料的物料
        /// </summary>
        [Description("已领料的物料")]
        public string MaterialCode { get; set; }
        /// <summary>
        ///已领料的物料描述
        /// </summary>
        [Description("已领料的物料描述")]
        public string MaterialDescription { get; set; }
        /// <summary>
        ///已领料的SN号和PN号
        /// </summary>
        [Description("已领料的SN号和PN号")]
        public string SNandPN { get; set; }
        /// <summary>
        ///需退的物料编码
        /// </summary>
        [Description("需退的物料")]
        public string ReplaceMaterialCode { get; set; }
        /// <summary>
        ///需退的物料描述
        /// </summary>
        [Description("需退的物料描述")]
        public string ReplaceMaterialDescription { get; set; }
        /// <summary>
        ///需退的SN号和PN号
        /// </summary>
        [Description("需退的SN号和PN号")]
        public string ReplaceSNandPN { get; set; }
    }
}
