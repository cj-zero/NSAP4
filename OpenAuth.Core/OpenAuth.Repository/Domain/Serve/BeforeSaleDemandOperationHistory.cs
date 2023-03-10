//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a CodeSmith Template.
//
//     DO NOT MODIFY contents of this file. Changes to this
//     file will be lost if the code is regenerated.
//     Author:Yubao Li
// </autogenerated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
	/// 售前需求对接申请流程操作历史记录
	/// </summary>
    [Table("beforesaledemandoperationhistory")]
    public partial class BeforeSaleDemandOperationHistory : Entity
    {
        public BeforeSaleDemandOperationHistory()
        {
          this.Action= string.Empty;
          this.CreateUser= string.Empty;
          this.CreateUserId= string.Empty;
          this.CreateTime= DateTime.Now;
          this.ApprovalResult= string.Empty;
          this.Remark= string.Empty;
        }
        
        /// <summary>
        /// 售前申请流程Id
        /// </summary>
        [Description("售前申请流程Id")]
        [Browsable(false)]
        public int BeforeSaleDemandId { get; set; }
        /// <summary>
        /// 操作行为
        /// </summary>
        [Description("操作行为")]
        public string Action { get; set; }
        /// <summary>
        /// 操作人
        /// </summary>
        [Description("操作人")]
        public string CreateUser { get; set; }
        /// <summary>
        /// 操作人Id
        /// </summary>
        [Description("操作人Id")]
        [Browsable(false)]
        public string CreateUserId { get; set; }
        /// <summary>
        /// 操作时间
        /// </summary>
        [Description("操作时间")]
        public System.DateTime? CreateTime { get; set; }
        /// <summary>
        /// 审批时长(分钟)
        /// </summary>
        [Description("审批时长(分钟)")]
        public int? IntervalTime { get; set; }
        /// <summary>
        /// 审批结果
        /// </summary>
        [Description("审批结果")]
        public string ApprovalResult { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Description("备注")]
        public string Remark { get; set; }
        /// <summary>
        /// 审批阶段
        /// </summary>
        [Description("审批阶段")]
        public int? ApprovalStage { get; set; }

        /// <summary>
        /// 售前需求流程申请审核附件
        /// </summary>
        public virtual List<BeforeSaleFiles> BeforeSaleFiles { get; set; }
    }
}