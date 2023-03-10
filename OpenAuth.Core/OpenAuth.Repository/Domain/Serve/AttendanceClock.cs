//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a CodeSmith Template.
//
//     DO NOT MODIFY contents of this file. Changes to this
//     file will be lost if the code is regenerated.
//     Author:Yubao Li
// </autogenerated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using AutoMapper.Configuration.Annotations;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
	/// 打卡记录表
	/// </summary>
    [Table("attendanceclock")]
    public partial class AttendanceClock : Entity
    {
        public AttendanceClock()
        {
            this.Name = string.Empty;
            this.UserId = string.Empty;
            this.Org = string.Empty;
            this.OrgId = string.Empty;
            this.ClockDate = DateTime.Now;
            this.Location = string.Empty;
            this.SpecificLocation = string.Empty;
            this.VisitTo = string.Empty;
            this.Remark = string.Empty;
            this.PhoneId = string.Empty;
            this.Ip = string.Empty;
            this.CreateTime = DateTime.Now;
        }


        /// <summary>
        /// 姓名
        /// </summary>
        [Description("姓名")]
        public string Name { get; set; }
        /// <summary>
        /// 打卡人用户Id
        /// </summary>
        [Description("打卡人用户Id")]
        [Browsable(false)]
        public string UserId { get; set; }
        /// <summary>
        /// 打卡人App用户Id
        /// </summary>
        [Description("打卡人App用户Id")]
        [Browsable(false)]
        public int AppUserId { get; set; }
        /// <summary>
        /// 部门
        /// </summary>
        [Description("部门")]
        public string Org { get; set; }
        /// <summary>
        /// 部门Id
        /// </summary>
        [Description("部门Id")]
        [Browsable(false)]
        public string OrgId { get; set; }
        /// <summary>
        /// 日期
        /// </summary>
        [Description("日期")]
        public System.DateTime? ClockDate { get; set; }
        /// <summary>
        /// 时间
        /// </summary>
        [Description("时间")]
        public System.TimeSpan? ClockTime { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        [Description("经度")]
        public decimal? Longitude { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        [Description("纬度")]
        public decimal? Latitude { get; set; }
        /// <summary>
        /// 地点
        /// </summary>
        [Description("地点")]
        public string Location { get; set; }
        /// <summary>
        /// 详细地点
        /// </summary>
        [Description("详细地点")]
        public string SpecificLocation { get; set; }
        /// <summary>
        /// 拜访对象
        /// </summary>
        [Description("拜访对象")]
        public string VisitTo { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Description("备注")]
        public string Remark { get; set; }
        /// <summary>
        /// 手机标识
        /// </summary>
        [Description("手机标识")]
        [Browsable(false)]
        public string PhoneId { get; set; }
        /// <summary>
        /// Ip地址
        /// </summary>
        [Description("Ip地址")]
        public string Ip { get; set; }
        /// <summary>
        /// 打卡类型（0：未知  1：签到  2：签退）
        /// </summary>
        [Description("打卡类型")]
        public int? ClockType { get; set; }

        /// <summary>
        /// 记录创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 打卡记录图片
        /// </summary>
        [Description("打卡记录图片")]
        [Ignore]
        public virtual List<AttendanceClockPicture> AttendanceClockPictures { get; set; }
    }
}