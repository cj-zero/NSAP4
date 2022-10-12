using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Serve
{
    /// <summary>
	/// 操作历史
	/// </summary>
    [Table("blamebelonghistory")]
    public class BlameBelongHistory : Entity
    {
        public BlameBelongHistory()
        {
            this.Action = string.Empty;
            this.CreateUser = string.Empty;
            this.CreateUserId = string.Empty;
            this.CreateTime = DateTime.Now;
            this.ApprovalResult = string.Empty;
            this.Remark = string.Empty;
        }

        /// <summary>
        /// 责任单ID
        /// </summary>
        [Description("责任单ID")]
        [Browsable(false)]
        public int BlameBelongId { get; set; }
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
        [Description("审批时长")]
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
    }
}
