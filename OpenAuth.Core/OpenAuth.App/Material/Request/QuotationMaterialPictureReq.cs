using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain.Material;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    [AutoMapTo(typeof(QuotationMaterialPicture))]
    public class QuotationMaterialPictureReq
    {
        /// <summary>
        /// 附件id
        /// </summary>
        public string PictureId { get; set; }
        /// <summary>
        /// 费用id
        /// </summary>
        public string QuotationMaterialId { get; set; }
        /// <summary>
        /// 附件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 附件类型
        /// </summary>
        public string FileType { get; set; }
    }
}
