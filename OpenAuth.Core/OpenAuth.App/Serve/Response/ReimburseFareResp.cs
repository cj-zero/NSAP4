using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Response
{
    [AutoMapTo(typeof(ReimburseFare))]
    public class ReimburseFareResp
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
        /// 交通类型
        /// </summary>
        public string TrafficType { get; set; }
        /// <summary>
        /// 交通工具
        /// </summary>
        public string Transport { get; set; }
        /// <summary>
        /// 出发地
        /// </summary>
        public string From { get; set; }
        /// <summary>
        /// 目的地
        /// </summary>
        public string To { get; set; }
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
        /// 开票日期
        /// </summary>
        public DateTime? InvoiceTime { get; set; }


        /// <summary>
        /// 出发地址经度
        /// </summary>
        public string FromLng { get; set; }

        /// <summary>
        /// 出发地址纬度
        /// </summary>
        public string FromLat { get; set; }

        /// <summary>
        /// 到达地址经度
        /// </summary>
        public string ToLng { get; set; }

        /// <summary>
        /// 到达地址纬度
        /// </summary>
        public string ToLat { get; set; }

        /// <summary>
        /// 费用归属
        /// </summary>
        public string ExpenseOrg { get; set; }
        /// <summary>
        /// 费用类型
        /// </summary>
        public string ExpenseType { get; set; }

        /// <summary>
        /// 附件
        /// </summary>
        public List<ReimburseAttachmentResp> ReimburseAttachments { get; set; }
        /// <summary>
        /// 费用归属
        /// </summary>
        public List<ReimburseExpenseOrgResp> ReimburseExpenseOrgs { get; set; }
        
    }
}
