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
    [Table("classroom_course_video_subject")]
    public class classroom_course_video_subject : BaseEntity<int>
    {
        /// <summary>
        /// 课程视频id
        /// </summary>
        public int CourseVideoId { get; set; }
        /// <summary>
        /// 题目id
        /// </summary>
        public int SubjectId { get; set; }
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
