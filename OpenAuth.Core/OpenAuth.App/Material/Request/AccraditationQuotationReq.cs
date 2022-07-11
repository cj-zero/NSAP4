using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    public class AccraditationQuotationReq
    {
        /// <summary>
        /// Id
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// AppId
        /// </summary>
        public int? AppId { get; set; }
        /// <summary>
        /// AppId
        /// </summary>
        public int? AppUserId { get; set; }

        /// <summary>
        /// 开票单位
        /// </summary>
        public string InvoiceCompany { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 是否驳回
        /// </summary>
        public bool IsReject { get; set; }

        /// <summary>
        /// 是否暂定
        /// </summary>
        public bool IsTentative { get; set; }

        /// <summary>
        /// 销售订单图片
        /// </summary>
        public List<string> PictureIds { get; set; }
    }
}
