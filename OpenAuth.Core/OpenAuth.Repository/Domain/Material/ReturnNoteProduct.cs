using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    ///  报价单设备列表
    /// </summary>
    [Table("ReturnNoteProduct")]
    public class ReturnNoteProduct : Entity
    {
        public ReturnNoteProduct()
        {
            this.ProductCode = string.Empty;
            this.MaterialCode = string.Empty;
            this.MaterialDescription = string.Empty;

        }

        /// <summary>
        ///退料单Id
        /// </summary>
        [Description("退料单Id")]
        public int ReturnNoteId { get; set; }

        /// <summary>
        ///产品编码
        /// </summary>
        [Description("产品编码")]
        public string ProductCode { get; set; }

        /// <summary>
        ///物料编码
        /// </summary>
        [Description("物料编码")]
        public string MaterialCode { get; set; }

        /// <summary>
        ///物料描述
        /// </summary>
        [Description("物料描述")]
        public string MaterialDescription { get; set; }

        /// <summary>
        /// 物料列表
        /// </summary>
        public virtual List<ReturnNoteMaterial> ReturnNoteMaterials { get; set; }

    }
}