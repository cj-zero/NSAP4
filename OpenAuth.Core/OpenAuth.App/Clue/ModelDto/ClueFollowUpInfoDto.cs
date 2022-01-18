using OpenAuth.Repository.Domain.Serve;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Clue.ModelDto
{
    public class ClueFollowUpInfoDto
    {
        public int Id { get; set; }
        /// <summary>
        /// 线索ID
        /// </summary>
        public int ClueId { get; set; }
        /// <summary>
        /// 联系人ID
        /// </summary>
        public int ContactsId { get; set; }
        /// <summary>
        /// 跟进方式（0：电话营销，1：邮件跟进，2：微信跟进，3：拜访客户，4，客户来访，5：其他）
        /// </summary>
        public int FollowUpWay { get; set; }
        /// <summary>
        /// 跟进内容
        /// </summary>
        public string Details { get; set; }
        /// <summary>
        /// 跟进时间
        /// </summary>
        public DateTime FollowUpTime { get; set; }
        /// <summary>
        /// 下次跟进时间
        /// </summary>
        public DateTime NextFollowTime { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public System.DateTime CreateTime { get; set; }
        /// <summary>
        /// 更新人
        /// </summary>
        public string UpdateUser { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 附件
        /// </summary>
        public List<ClueFile> ClueFileList { get; set; } = new List<ClueFile>();
    }
}
