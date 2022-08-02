using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 
    /// </summary>
    [Table("classroom_course_video")]
    public class classroom_course_video : BaseEntity<int>
    {
        /// <summary>
        /// 视频名字
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 课程Id
        /// </summary>
        public int CourseId { get; set; }
        /// <summary>
        /// 视频时长
        /// </summary>
        public int Duration { get; set; }
        /// <summary>
        /// 视频地址
        /// </summary>
        public string VideoUrl { get; set; }
        /// <summary>
        /// 视频浏览量
        /// </summary>
        public int ViewedCount { get; set; }
        /// <summary>
        /// 视频开放状态
        /// </summary>
        public bool State { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime CreateTime { get; set; }
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
