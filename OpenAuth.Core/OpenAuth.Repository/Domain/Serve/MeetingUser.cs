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
    [Table("meetinguser")]
    public class MeetingUser : BaseEntity<int>
    {
        /// <summary>
        /// 会议Id
        /// </summary>
        [Description("会议Id")]
        public int MeetingId { get; set; }
        /// <summary>
        /// 用户Id
        /// </summary>
        [Description("用户Id")]
        public int UserId { get; set; }
        /// <summary>
        /// 报名人
        /// </summary>
        [Description("报名人")]
        public string Name { get; set; }
        /// <summary>
        /// 部门Id
        /// </summary>
        [Description("部门Id")]
        public int DempId { get; set; }
        /// <summary>
        /// 部门
        /// </summary>
        [Description("部门")]
        public string DempName { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Description("备注")]
        public string Remark { get; set; }
        /// <summary>
        /// 审核状态(0:待审核，1:审核通过，-1：驳回，2:取消)
        /// </summary>
        [Description("审核状态")]
        public int Status { get; set; }
        /// <summary>
        /// 报名时间
        /// </summary>
        [Description("报名时间")]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        [Description("创建人")]
        public string CreateUser { get; set; }
        /// <summary>
        /// 取消原因
        /// </summary>
        [Description("取消原因")]
        public string CancelReason { get; set; }
        /// <summary>
        /// 取消时间
        /// </summary>
        [Description("取消时间")]
        public DateTime CancelTime { get; set; }
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
