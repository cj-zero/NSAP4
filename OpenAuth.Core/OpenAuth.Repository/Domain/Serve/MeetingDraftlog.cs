using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Serve
{
    [Table("meetingdraftlog")]

    public class MeetingDraftlog : BaseEntity<int>
    {
        /// <summary>
        /// 草稿Id
        /// </summary>
        [Description("草稿Id")]
        public int DraftId { get; set; }
        /// <summary>
        /// 日志
        /// </summary>
        [Description("日志")]
        public string Log { get; set; }
        /// <summary>
        /// 操作人
        /// </summary>
        [Description("操作人")]
        public string CreateUser { get; set; }
        /// <summary>
        /// 操作时间
        /// </summary>
        [Description("操作时间")]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        [Description("是否删除")]
        public bool IsDelete { get; set; }
        /// <summary>
        /// 类型 1：会议申请审核流程，2：会议取消审核流程，3：报名审核流程，4：取消审核流程
        /// </summary>
        public int Type { get; set; }
        public override void GenerateDefaultKeyVal()
        {

        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
