using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Material
{
    [Table("quotationpicture")]
    public class QuotationPicture : Entity
    {
        public QuotationPicture()
        {

        }

        /// <summary>
        ///物料单id
        /// </summary>
        [Description("物料单id")]
        public int? QuotationId { get; set; }

        /// <summary>
        ///文件id
        /// </summary>
        [Description("文件id")]
        public string PictureId { get; set; }
    }
}
