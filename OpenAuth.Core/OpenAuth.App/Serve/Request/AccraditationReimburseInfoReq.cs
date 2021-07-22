using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    /// <summary>
    /// 
    /// </summary>
    [Table("ReimburseInfo")]
    public class AccraditationReimburseInfoReq
    {
        /// <summary>
        /// 报销单单个Id
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// 客户简称
        /// </summary>
        public string ShortCustomerName { get; set; }
        /// <summary>
        /// 报销类别
        /// </summary>
        public string ReimburseType { get; set; }
        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// 费用承担
        /// </summary>
        public string BearToPay { get; set; }
        /// <summary>
        /// 责任承担
        /// </summary>
        public string Responsibility { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 流程id
        /// </summary>
        public string FlowInstanceId { get; set; }

        /// <summary>
        /// 是否驳回
        /// </summary>
        public bool IsReject { get; set; }


        /// <summary>
        /// 报销单多个Id
        /// </summary>
        public List<int> ReimburseId { get; set; }

        public List<OrgResult> travelOrgResults { get; set; }
        public List<OrgResult> transportOrgResults { get; set; }
        public List<OrgResult> hotelOrgResults { get; set; }
        public List<OrgResult> otherOrgResults { get; set; }

        /// <summary>
        /// 费用归属
        /// </summary>
        public List<ReimburseExpenseOrgReq> ReimburseExpenseOrgs { get; set; }
    }

    public class OrgResult
    {

        public int Id { get; set; }

        public string Value { get; set; }
    }
}
