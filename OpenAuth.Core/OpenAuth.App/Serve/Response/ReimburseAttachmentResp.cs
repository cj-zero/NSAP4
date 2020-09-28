using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Response
{
    [AutoMapTo(typeof(ReimburseAttachment))]
    public class ReimburseAttachmentResp
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 文件Id
        /// </summary>
        public string FileId { get; set; }
        /// <summary>
        /// 关联表主键Id
        /// </summary>
        public int ReimburseId { get; set; }
        /// <summary>
        /// 报销单据类型(1 出差补贴， 2 交通费用， 3住宿补贴， 4 其他费用 5我的费用)
        /// </summary>
        public int? ReimburseType { get; set; }
        /// <summary>
        /// 附件类型(1 普通附件， 2 发票附件)
        /// </summary>
        public int? AttachmentType { get; set; }

        /// <summary>
        /// 附件名称
        /// </summary>
        public string AttachmentName { get; set; }
    }
}
