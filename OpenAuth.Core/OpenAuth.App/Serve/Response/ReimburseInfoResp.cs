using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Response
{
    /// <summary>
    /// 
    /// </summary>
    [AutoMapTo(typeof(ReimburseInfo))]
    public class ReimburseInfoResp
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 报销单号
        /// </summary>
        public int MainId { get; set; }
        /// <summary>
        /// 服务单主键Id
        /// </summary>
        public int ServiceOrderId { get; set; }
        /// <summary>
        /// SAP服务Id
        /// </summary>
        public int ServiceOrderSapId { get; set; }
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
        /// 报销状态
        /// </summary>
        public int RemburseStatus { get; set; }
        /// <summary>
        /// 费用承担
        /// </summary>
        public string BearToPay { get; set; }
        /// <summary>
        /// 责任承担
        /// </summary>
        public string Responsibility { get; set; }
        /// <summary>
        /// 劳务关系
        /// </summary>
        public string ServiceRelations { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 总金额
        /// </summary>
        public decimal? TotalMoney { get; set; }

        /// <summary>
        /// 部门承担金额
        /// </summary>
        public decimal? Money { get; set; }
        /// <summary>
        /// 支付时间
        /// </summary>
        public System.DateTime? PayTime { get; set; }
        /// <summary>
        /// 是否草稿状态
        /// </summary>
        public bool IsDraft { get; set; }
        /// <summary>
        /// 工作流程Id
        /// </summary>
        public string FlowInstanceId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public System.DateTime CreateTime { get; set; }
        /// <summary>
        /// 创建人Id
        /// </summary>
        public string CreateUserId { get; set; }

        /// <summary>
        /// 是否已读
        /// </summary>
        public int? IsRead { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public System.DateTime UpdateTime { get; set; }

        /// <summary>
        /// 出差补贴
        /// </summary>
        public virtual List<ReimburseTravellingAllowanceResp> ReimburseTravellingAllowances { get; set; }
        /// <summary>
        /// 差旅报销其他费用
        /// </summary>
        public virtual List<ReimburseOtherChargesResp> ReimburseOtherCharges { get; set; }
        /// <summary>
        /// 差旅报销交通费用
        /// </summary>
        public virtual List<ReimburseFareResp> ReimburseFares { get; set; }
        /// <summary>
        /// 差旅报销住宿补贴
        /// </summary>
        public virtual List<ReimburseAccommodationSubsidyResp> ReimburseAccommodationSubsidies { get; set; }
        /// <summary>
        /// 差旅报销单操作历史
        /// </summary>
        public virtual List<ReimurseOperationHistory> ReimurseOperationHistories { get; set; }
        /// <summary>
        /// 差旅报销单附件表
        /// </summary>
        public virtual List<ReimburseAttachmentResp> ReimburseAttachments { get; set; }
    }
}
