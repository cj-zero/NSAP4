using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Settlement.Request
{
    public class AccraditationOutsourcReq
    {
        /// <summary>
        /// 结算单id
        /// </summary>
        public string OutsourcId { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 是否驳回
        /// </summary>
        public bool IsReject { get; set; }
        /// <summary>
        /// 工时
        /// </summary>
        public int? ManHour { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public string Money { get; set; }
        /// <summary>
        /// 费用归属
        /// </summary>
        public List<ReimburseExpenseOrgReq> OutsourcExpenseOrgReqs { get; set; }

        /// <summary>
        /// 费用单id
        /// </summary>
        public string outsourcexpensesId { get; set; }
    }
}
