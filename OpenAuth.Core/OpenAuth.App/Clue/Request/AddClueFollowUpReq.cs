using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Clue.Request
{
    /// <summary>
    /// 新增跟进model
    /// </summary>
    public class AddClueFollowUpReq
    {
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
        public string FollowUpTime { get; set; }
        /// <summary>
        /// 下次跟进时间
        /// </summary>
        public string NextFollowTime { get; set; }
        /// <summary>
        /// 附件
        /// </summary>
        public List<AddClueFileUploadReq> AddClueFileUploadReq { get; set; }
    }
}
