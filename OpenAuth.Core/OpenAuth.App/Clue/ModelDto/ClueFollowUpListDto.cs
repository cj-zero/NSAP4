using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Clue.ModelDto
{
    /// <summary>
    /// 跟进列表
    /// </summary>
    public class ClueFollowUpListDto
    {
        public int Id { get; set; }
        /// <summary>
        /// 线索ID
        /// </summary>
        public int ClueId { get; set; }
        /// <summary>
        /// 联系人Name
        /// </summary>
        public string Name { get; set; }
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
    }
}
