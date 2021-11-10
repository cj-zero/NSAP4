using OpenAuth.Repository.Domain.Serve;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Meeting.ModelDto
{
    public class ExhibitionDetailDto
    {
        public int Id { get; set; }

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
        ///  0：草稿，1：待审，2：审核通过，3：审批通过，4 ：拒绝，驳回：6
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
        public int DempId { get; set; }
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
        /// 主办单位
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
        public string MeasureOfArea { get; set; }
        /// <summary>
        /// 展品类别
        /// </summary>
        public string ProductType { get; set; }
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
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 更新人
        /// </summary>
        public string UpdateUser { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 取消原因
        /// </summary>
        public string CancelReason { get; set; }
        /// <summary>
        /// 取消时间
        /// </summary>
        public DateTime? CancelTime { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { get; set; } = false;
        public List<MeetingFile> FileList { get; set; }
        /// <summary>
        /// 报名信息
        /// </summary>
        public MeetingUserDto meetingUser { get; set; }

    }
}
