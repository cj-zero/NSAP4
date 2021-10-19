using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Serve
{
    /// <summary>
    /// 展会
    /// </summary>
    [Table("meeting")]
    public partial class Meetings : BaseEntity<int>
    {
        /// <summary>
        /// 展会名称
        /// </summary>
        [Description("展会名称")]
        public string Name { get; set; }
        /// <summary>
        /// 会议主题
        /// </summary>
        [Description("会议主题")]
        public string Title { get; set; }
        /// <summary>
        /// 会议概况
        /// </summary>
        [Description("会议概况")]
        public string Introduce { get; set; }
        /// <summary>
        /// 展会开始时间
        /// </summary>
        [Description("展会开始时间")]
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 展会结束时间
        /// </summary>
        [Description("展会结束时间")]
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 1：国内，2：国外
        /// </summary>
        [Description("展会地点")]
        public int AddressType { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        [Description("地址")]
        public string Address { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        [Description("状态")]
        public int Status { get; set; }
        /// <summary>
        /// 申请人ID
        /// </summary>
        [Description("申请人ID")]
        public int ApplyUserId { get; set; }
        /// <summary>
        /// 申请人
        /// </summary>
        [Description("申请人")]
        public string ApplyUser { get; set; }
        /// <summary>
        /// 申请部门ID
        /// </summary>
        [Description("申请部门ID")]
        public int DempIdtId { get; set; }
        /// <summary>
        /// 申请部门
        /// </summary>
        [Description("申请部门")]
        public string ApplyDempName { get; set; }
        /// <summary>
        /// 联系方式
        /// </summary>
        [Description("联系方式")]
        public string Contact { get; set; }
        /// <summary>
        /// 跟进人
        /// </summary>
        [Description("跟进人")]
        public string FollowPerson { get; set; }
        /// <summary>
        /// 主办单位
        /// </summary>
        [Description("主办单位")]
        public string SponsorUnit { get; set; }
        /// <summary>
        /// 指导单位
        /// </summary>
        [Description("指导单位")]
        public string GuideUnit { get; set; }
        /// <summary>
        /// 申请理由
        /// </summary>
        [Description("申请理由")]
        public string ApplyReason { get; set; }
        /// <summary>
        /// 会议规模
        /// </summary>
        [Description("会议规模")]
        public int ConferenceScale { get; set; }
        /// <summary>
        /// 人数限制
        /// </summary>
        [Description("人数限制")]
        public int UserNumberLimit { get; set; }
        /// <summary>
        /// 经费预算
        /// </summary>
        [Description("经费预算")]
        public decimal Funds { get; set; }
        /// <summary>
        /// 展位位置
        /// </summary>
        [Description("展位位置")]
        public string Position { get; set; }
        /// <summary>
        /// 展位面积
        /// </summary>
        [Description("展位面积")]
        public double MeasureOfArea { get; set; }
        /// <summary>
        /// 展品类别
        /// </summary>
        [Description("展品类别")]
        public int ProductType { get; set; }
        /// <summary>
        /// 有无晚宴
        /// </summary>
        [Description("有无晚宴")]
        public bool IsDinner { get; set; }
        /// <summary>
        /// 展位搭建
        /// </summary>
        [Description("展位搭建")]
        public bool BulidType { get; set; }
        /// <summary>
        /// 启用签到
        /// </summary>
        [Description("启用签到")]
        public int IsSign { get; set; }
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
        /// 取消原因
        /// </summary>
        [Description("取消原因")]
        public string CancelReason { get; set; }
        /// <summary>
        /// 取消时间
        /// </summary>
        [Description("取消时间")]
        public DateTime? CancelTime { get; set; }
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
