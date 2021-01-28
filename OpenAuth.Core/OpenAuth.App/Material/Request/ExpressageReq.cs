using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    /// <summary>
    /// 物流单
    /// </summary>
    [Table("Expressage")]
    [AutoMapTo(typeof(Expressage))]
    public partial class ExpressageReq
    {
        /// <summary>
        ///id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        ///快递单号
        /// </summary>
        public string ExpressNumber { get; set; }

        /// <summary>
        ///退料单Id
        /// </summary>
        public int? ReturnNoteId { get; set; }

        /// <summary>
        ///报价单Id
        /// </summary>
        public int? QuotationId { get; set; }

        /// <summary>
        ///物流信息
        /// </summary>
        public string ExpressInformation { get; set; }
        /// <summary>
        ///备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        ///运费
        /// </summary>
        public string Freight { get; set; }
        
        /// <summary>
        /// 物流图片
        /// </summary>
        public virtual List<string> ExpressagePictures { get; set; }
    }
}
