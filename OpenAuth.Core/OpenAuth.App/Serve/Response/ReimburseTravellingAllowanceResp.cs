using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Response
{
    [AutoMapTo(typeof(ReimburseTravellingAllowance))]
    public class ReimburseTravellingAllowanceResp
    {
        /// <summary>
        /// Id
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
        /// 天数
        /// </summary>
        public int? Days { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public decimal? Money { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 费用归属
        /// </summary>
        public string ExpenseOrg { get; set; }

        /// <summary>
        /// 是否客服新增
        /// </summary>
        public bool? IsAdded { get; set; }

        /// <summary>
        /// 费用归属
        /// </summary>
        public List<ReimburseExpenseOrgResp> ReimburseExpenseOrgs { get; set; }
    }
}
