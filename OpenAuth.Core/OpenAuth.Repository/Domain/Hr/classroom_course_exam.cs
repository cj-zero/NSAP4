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
    [Table("classroom_course_exam")]
    public class classroom_course_exam:BaseEntity<int>
    {
        /// <summary>
        /// 
        /// </summary>
        public int AppUserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int CoursePackageId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int CourseId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int CourseVideoId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal TotalScore { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsPass { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsSubmit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime SubmitTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal TestScores { get; set; }


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
