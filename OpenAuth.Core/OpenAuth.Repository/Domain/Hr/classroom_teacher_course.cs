using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 讲师开课课程表
    /// </summary>
    [Table("classroom_teacher_course")]
    public class classroom_teacher_course : BaseEntity<int>
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 使用人群
        /// </summary>
        public string ForTheCrowd { get; set; }
        /// <summary>
        /// 教学方式 1:线上  2:线下
        /// </summary>
        public int TeachingMethod { get; set; }
        /// <summary>
        /// 教学地址
        /// </summary>
        public string TeachingAddres { get; set; }
        /// <summary>
        /// 开播用户
        /// </summary>
        public int AppUserId { get; set; }
        /// <summary>
        /// 背景图
        /// </summary>
        public string BackgroundImage { get; set; }
        /// <summary>
        /// 视频地址
        /// </summary>
        public string VideoUrl { get; set; }
        /// <summary>
        /// 审核状态 1:未审核 2:审核已通过 3:已驳回 4:封禁
        /// </summary>
        public int AuditState { get; set; }
        /// <summary>
        /// 申请时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 观看次数
        /// </summary>
        public int ViewedCount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public override void GenerateDefaultKeyVal()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
