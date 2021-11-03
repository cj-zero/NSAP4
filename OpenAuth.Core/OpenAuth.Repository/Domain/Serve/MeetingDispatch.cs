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
    [Table("meetingdispatch")]
    public class MeetingDispatch : BaseEntity<int>
    {
        /// <summary>
        /// 原会议ID
        /// </summary>
        [Description("原会议ID")]

        public int FromMeetingId { get; set; }
        /// <summary>
        /// 新会议Id
        /// </summary>
        [Description("新会议Id")]

        public int ToMeetingId { get; set; }
        /// <summary>
        /// 调度人员
        /// </summary>
        [Description("调度人员")]

        public string UserJson { get; set; }
        /// <summary>
        /// 调度原因
        /// </summary>
        [Description("调度原因")]

        public string Reason { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        [Description("创建人")]
        public string CreateUser { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public System.DateTime CreateTime { get; set; }
        public override void GenerateDefaultKeyVal()
        {

        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
