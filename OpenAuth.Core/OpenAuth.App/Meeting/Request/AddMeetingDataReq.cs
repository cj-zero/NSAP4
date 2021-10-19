using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Meeting.Request
{
    public class AddMeetingDataReq
    {
        /// <summary>
        /// 展会名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 会议主题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 会议概况
        /// </summary>
        public string Introduce { get; set; }
        /// <summary>
        /// 展会开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 展会结束时间
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 1：国内，2：国外
        /// </summary>
        public int AddressType { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 申请人ID
        /// </summary>
        public int ApplyUserId { get; set; }
        /// <summary>
        /// 申请人
        /// </summary>
        public string ApplyUser { get; set; }
        /// <summary>
        /// 申请部门ID
        /// </summary>
        public int DempIdtId { get; set; }
        /// <summary>
        /// 申请部门
        /// </summary>
        public string ApplyDempName { get; set; }
        /// <summary>
        /// 联系方式
        /// </summary>
        public string Contact { get; set; }
        /// <summary>
        /// 跟进人
        /// </summary>
        public string FollowPerson { get; set; }
        /// <summary>
        /// 主板单位
        /// </summary>
        public string SponsorUnit { get; set; }
        /// <summary>
        /// 指导单位
        /// </summary>
        public string GuideUnit { get; set; }
        /// <summary>
        /// 申请理由
        /// </summary>
        public string ApplyReason { get; set; }
        /// <summary>
        /// 会议规模
        /// </summary>
        public int ConferenceScale { get; set; }
        /// <summary>
        /// 人数限制
        /// </summary>
        public int UserNumberLimit { get; set; }
        /// <summary>
        /// 经费预算
        /// </summary>
        public decimal Funds { get; set; }
        /// <summary>
        /// 展位位置
        /// </summary>
        public string Position { get; set; }
        /// <summary>
        /// 展位面积
        /// </summary>
        public double MeasureOfArea { get; set; }
        /// <summary>
        /// 展品类别
        /// </summary>
        public int ProductType { get; set; }
        /// <summary>
        /// 有无晚宴
        /// </summary>
        public bool IsDinner { get; set; }
        /// <summary>
        /// 展位搭建
        /// </summary>
        public bool BulidType { get; set; }
        /// <summary>
        /// 启用签到
        /// </summary>
        public int IsSign { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 附件
        /// </summary>
        public List<FileReq> FileList { get; set; }
    }
}
