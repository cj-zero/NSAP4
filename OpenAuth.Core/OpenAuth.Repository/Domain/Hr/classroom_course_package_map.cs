using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 课程包课程关系表
    /// </summary>
    [Table("classroom_course_package_map")]
    public class classroom_course_package_map : BaseEntity<int>
    {
        /// <summary>
        /// 课程包Id
        /// </summary>
        public int CoursePackageId { get; set; }
        /// <summary>
        /// 课程Id
        /// </summary>
        public int CourseId { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
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
