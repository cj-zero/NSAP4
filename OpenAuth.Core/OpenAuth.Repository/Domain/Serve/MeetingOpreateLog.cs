using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Serve
{
    /// <summary>
    /// 调度
    /// </summary>
    [Table("meetingopreatelog")]
    public partial class MeetingOpreateLog : BaseEntity<int>
    {
        /// <summary>
        /// 会议Id
        /// </summary>
        [Description("会议Id")]
        public int MeetingId { get; set; }
        /// <summary>
        /// 日志
        /// </summary>
        [Description("日志")]
        public string Log { get; set; }
        /// <summary>
        /// 修改内容
        /// </summary>
        [Description("修改内容")]
        public string Json { get; set; }
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
        public override void GenerateDefaultKeyVal()
        {

        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
