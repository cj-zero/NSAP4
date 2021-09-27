using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain.Material;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    /// <summary>
    /// 物料更换记录
    /// </summary>
    [Table("MaterialReplaceRecord")]
    [AutoMapTo(typeof(MaterialReplaceRecord))]
    public partial class AddOrUpdateMaterialReplaceRecordReq
    {

        /// <summary>
        ///appid
        /// </summary>
        public int? AppUserId { get; set; }
        /// <summary>
        /// 更换物料信息
        /// </summary>
        public List<MaterialReplaceRecordReq> MaterialReplaceRecordReqs { get; set; }
    }

    public partial class MaterialReplaceRecordReq
    {
        public int LineNum { get; set; }
        public string Status { get; set; }
        public decimal? Count { get; set; }
        /// <summary>
        /// 物料类型
        /// </summary>
        public int? MaterialType { get; set; }
        /// <summary>
        ///物料报价单Id
        /// </summary>
        public int QuotationId { get; set; }
        /// <summary>
        ///序列号
        /// </summary>
        public string ProductCode { get; set; }
        /// <summary>
        ///已领料的物料
        /// </summary>
        public string MaterialCode { get; set; }
        /// <summary>
        ///已领料的物料描述
        /// </summary>
        public string MaterialDescription { get; set; }

        /// <summary>
        ///已领料的SN号和PN号
        /// </summary>
        public string SNandPN { get; set; }
        /// <summary>
        ///需退的物料
        /// </summary>
        public string ReplaceMaterialCode { get; set; }
        /// <summary>
        ///需退的物料描述
        /// </summary>
        public string ReplaceMaterialDescription { get; set; }
        /// <summary>
        ///需退的SN号和PN号
        /// </summary>
        public string ReplaceSNandPN { get; set; }
    }
}
