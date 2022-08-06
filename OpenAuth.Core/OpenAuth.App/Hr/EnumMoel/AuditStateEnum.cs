using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace OpenAuth.App.Hr
{
    /// <summary>
    /// 审核状态枚举
    /// </summary>
    public enum AuditStateEnum
    {
        /// <summary>
        /// 待审核
        /// </summary>
        [Description("待审核")]
        ToBeReviewed = 1,

        /// <summary>
        /// 已通过
        /// </summary>
        [Description("已通过")]
        Normal = 2,

        /// <summary>
        /// 已驳回
        /// </summary>
        [Description("已驳回")]
        Rejected = 3,

        /// <summary>
        /// 封禁
        /// </summary>
        [Description("封禁")]
        Ban = 4,
    }
}
