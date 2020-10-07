using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Response
{
    [AutoMapTo(typeof(ReimburseOtherCharges))]
    public class ReimburseOtherChargesResp
    {
        /// <summary>
        /// ID
        /// </summary>
        public int? Id { get; set; }
        /// <summary>
        /// 报销单ID
        /// </summary>
        public int? ReimburseInfoId { get; set; }
        /// <summary>
        /// 序号
        /// </summary>
        public int? SerialNumber { get; set; }
        /// <summary>
        /// 费用类别
        /// </summary>
        public string ExpenseCategory { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public decimal? Money { get; set; }
        /// <summary>
        /// 发票号码
        /// </summary>
        public string InvoiceNumber { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 我的费用Id
        /// </summary>
        public string MyExpendsId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 是否添加
        /// </summary>
        public bool? IsAdd { get; set; }

        /// <summary>
        /// 附件
        /// </summary>
        public List<ReimburseAttachmentResp> ReimburseAttachments { get; set; }
    }
}
