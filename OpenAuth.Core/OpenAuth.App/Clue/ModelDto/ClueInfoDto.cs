using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Clue.ModelDto
{
    /// <summary>
    /// 线索详情model
    /// </summary>
    public class ClueInfoDto
    {
        /// <summary>
        /// 线索编号
        /// </summary>
        public string SerialNumber { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 线索归属
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; }
        /// <summary>
        /// 跟进时间
        /// </summary>
        public string FollowUpTime { get; set; }
        /// <summary>
        /// 基本信息
        /// </summary>
        public Essential Essential { get; set; } = new Essential();
        /// <summary>
        /// 操作记录
        /// </summary>
        public List<Log> Log { get; set; }=new List<Log>();
        /// <summary>
        /// 日程
        /// </summary>
        public List<Schedule> Schedule { get; set; }=new List<Schedule>();
        /// <summary>
        /// 跟进记录
        /// </summary>
        public List<FollowUp> FollowUp { get; set; }=new List<FollowUp>();
        /// <summary>
        /// 意向商品
        /// </summary>
        public List<IntentionProduct> IntentionProduct { get; set; }=new List<IntentionProduct>();
    }
    /// <summary>
    /// 基本信息
    /// </summary>
    public class Essential
    {
        /// <summary>
        /// 客户来源 0:领英、1:国内展会、2:国外展会、3:客户介绍、4:新威官网、5:其他
        /// </summary>
        public int CustomerSource { get; set; }

        /// <summary>
        /// 所属行业
        /// </summary>
        public int IndustryInvolved { get; set; }
        /// <summary>
        /// 人员规模
        /// </summary>
        public int StaffSize { get; set; }
        /// <summary>
        /// 网址
        /// </summary>
        public string WebSite { get; set; }
        /// <summary>
        /// 标签集合
        /// </summary>
        public string Tags { get; set; }
        /// <summary>
        ///备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 联系人名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 联系方式1
        /// </summary>
        public string Tel1 { get; set; }
        /// <summary>
        /// 详细地址(国家省市)
        /// </summary>
        public string Address1 { get; set; }
        /// <summary>
        /// 详细地址
        /// </summary>
        public string Address2 { get; set; }
        /// <summary>
        /// 角色（0：决策者、1：普通人）
        /// </summary>
        public int Role { get; set; }
        /// <summary>
        ///职位
        /// </summary>
        public string Position { get; set; }
    }
    /// <summary>
    /// 操作记录
    /// </summary>
    public class Log
    {
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 操作内容
        /// </summary>
        public string Details { get; set; }
        /// <summary>
        /// 操作类型（0：新增，1：编辑，2：删除）
        /// </summary>
        public int LogType { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
    /// <summary>
    /// 日程
    /// </summary>
    public class Schedule
    {
        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime { get; set; }
        /// <summary>
        /// 日程内容
        /// </summary>
        public string Details { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 状态
        /// 状态（状态（0：未完成，1：已完成））
        /// </summary>
        public int Status { get; set; }
    }
    /// <summary>
    /// 跟进记录
    /// </summary>
    public class FollowUp
    {
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 跟进方式（0：电话营销，1：邮件跟进，2：微信跟进，3：拜访客户，4，客户来访，5：其他）
        /// </summary>
        public int FollowUpWay { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 跟进时间
        /// </summary>
        public string FollowUpTime { get; set; }
    }
    /// <summary>
    /// 意向商品
    /// </summary>
    public class IntentionProduct
    {
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 物料描述
        /// </summary>
        public string ItemDescription { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        public string Pic { get; set; }
    }
}
