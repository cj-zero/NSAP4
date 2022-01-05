using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Serve
{
    /// <summary>
    /// 线索日程
    /// </summary>
    [Table("clueschedule")]
    public class ClueSchedule : BaseEntity<int>
    {
        /// <summary>
        /// 线索ID
        /// </summary>
        [Description("线索ID")]
        public int ClueId { get; set; }
        /// <summary>
        /// 日程内容
        /// </summary>
        [Description("日程内容")]
        public string Details { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        [Description("开始时间")]
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        [Description("结束时间")]
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 参与人员
        /// </summary>
        [Description("参与人员")]
        public string Participant { get; set; }
        /// <summary>
        /// 提醒时间
        /// </summary>
        [Description("提醒时间")]
        public DateTime RemindTime { get; set; }
        /// <summary>
        /// 相关对象
        /// </summary>
        [Description("相关对象")]
        public string RelatedObjects { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Description("备注")]
        public string Remark { get; set; }
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
        /// <summary>
        /// 更新人
        /// </summary>
        [Description("更新人")]
        public string UpdateUser { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        [Description("更新时间")]
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 状态
        /// 状态（状态（0：未完成，1：已完成））
        /// </summary>
        [Description("状态")]
        public int Status { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        [Description("是否删除")]
        public bool IsDelete { get; set; } = false;

        public override void GenerateDefaultKeyVal()
        {

        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
