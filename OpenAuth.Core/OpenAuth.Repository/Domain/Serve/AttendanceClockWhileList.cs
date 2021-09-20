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
	/// 打卡推送白名单
	/// </summary>
    [Table("attendanceclockwhilelist")]
    public partial class AttendanceClockWhileList : Entity
    {
        public AttendanceClockWhileList() 
        {

        }
        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 是否开启
        /// </summary>
        public bool IsEnable { get; set; }
        /// <summary>
        /// 1-配置名单 2-有服务ID的人
        /// </summary>
        public int Type { get; set; }
    }
}
