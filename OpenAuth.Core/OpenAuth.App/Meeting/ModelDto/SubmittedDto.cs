using AutoMapper;
using OpenAuth.Repository.Domain.Serve;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Meeting.ModelDto
{
    [AutoMap(typeof(MeetingDraft))]
    public class SubmittedDto
    {
        /// <summary>
        /// 审批序号
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 单据类型
        /// 0 :展会申请 1：报名申请
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 原单据编号
        /// </summary>
        public int Base_entry { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 流程步骤
        /// 0 草稿，1：审核中（主管审批） 2：审核中（or审批），3：不批准，4，已批准
        /// </summary>
        public int Step { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 会议名称
        /// </summary>
        public string MeetingName { get; set; }
        /// <summary>
        /// 0：国内，1：国外
        /// </summary>
        public int AddressType { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 展会开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 展会结束时间
        /// </summary>
        public DateTime EndTime { get; set; }
    }
}
