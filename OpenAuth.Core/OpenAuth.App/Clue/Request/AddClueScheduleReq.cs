using OpenAuth.App.Meeting.ModelDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Clue.Request
{
    /// <summary>
    /// 新增日程model
    /// </summary>
    public class AddClueScheduleReq
    {
        /// <summary>
        /// 
        /// </summary>
        public int ClueId { get; set; }
        /// <summary>
        /// 日程内容
        /// </summary>
        public string Details { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime { get; set; }
        /// <summary>
        /// 参与人员
        /// </summary>
        public List<TextVaule> Participant { get; set; }
        /// <summary>
        /// 提醒时间
        /// </summary>
        public string  RemindTime { get; set; }
        /// <summary>
        /// 相关对象0:客户，1报价单，2：订单
        /// </summary>
        public int RelatedObjects { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}
