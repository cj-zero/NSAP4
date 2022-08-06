using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Hr
{
    /// <summary>
    /// 专题课程包开放用户表
    /// </summary>
    [Table("classroom_subject_course_user")]
    public class classroom_subject_course_user : BaseEntity<int>
    {
        /// <summary>
        /// App用户id
        /// </summary>
        public int AppUserId { get; set; }

        /// <summary>
        /// 专题Id
        /// </summary>
        public int SubjectId { get; set; }

        /// <summary>
        /// 专题课程Id
        /// </summary>
        public int SubjectCourseId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 课程学习进度(秒s)
        /// </summary>
        public int Schedule { get; set; }

        /// <summary>
        /// 是否完成  
        /// </summary>
        public bool IsComplete { get; set; }

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
