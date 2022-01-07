using OpenAuth.App.Meeting.ModelDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Clue.ModelDto
{
    /// <summary>
    /// 日程详情Dto
    /// </summary>
    public class ClueScheduleListDto
    {
        public int Id { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 日程内容
        /// </summary>
        public string Details {  get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CardName {  get; set; }
        /// <summary>
        /// 相关对象
        /// </summary>
        public int RelatedObjects { get; set; }
        /// <summary>
        /// 参与人员
        /// </summary>
        public List<TextVaule> Participant { get; set; }
        /// <summary>
        /// 提醒时间
        /// </summary>
        public DateTime RemindTime { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 状态
        /// 状态（状态（0：未完成，1：已完成））
        /// </summary>
        public int Status { get; set; }
    }
}
