using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Serve
{
    /// <summary>
    /// 线索跟进
    /// </summary>
    [Table("cluefollowup")]
    public class ClueFollowUp : BaseEntity<int>
    {
        /// <summary>
        /// 线索ID
        /// </summary>
        [Description("线索ID")]
        public int ClueId { get; set; }
        /// <summary>
        /// 联系人ID
        /// </summary>
        [Description("联系人ID")]
        public int ContactsId { get; set; }
        /// <summary>
        /// 跟进方式（0：电话营销，1：邮件跟进，2：微信跟进，3：拜访客户，4，客户来访，5：其他）
        /// </summary>
        [Description("跟进方式（0：电话营销，1：邮件跟进，2：微信跟进，3：拜访客户，4，客户来访，5：其他）")]
        public int FollowUpWay { get; set; }
        /// <summary>
        /// 跟进内容
        /// </summary>
        [Description("跟进内容")]
        public string Details { get; set; }
        /// <summary>
        /// 跟进时间
        /// </summary>
        [Description("跟进时间")]
        public DateTime FollowUpTime { get; set; }
        /// <summary>
        /// 下次跟进时间
        /// </summary>
        [Description("下次跟进时间")]
        public DateTime NextFollowTime { get; set; }
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
