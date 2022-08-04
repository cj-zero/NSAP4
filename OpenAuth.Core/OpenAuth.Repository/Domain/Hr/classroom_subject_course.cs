using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;


namespace OpenAuth.Repository.Domain.Hr
{
    /// <summary>
    /// 专题课程表
    /// </summary>
    [Table("classroom_subject_course")]
    public class classroom_subject_course : BaseEntity<int>
    {
        /// <summary>
        /// 专题Id
        /// </summary>
        public int SubjectId { get; set; }
        /// <summary>
        /// 课程名字
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 上架时间
        /// </summary>
        public DateTime ShelfTime { get; set; }
 
        /// <summary>
        /// 类型 1=文本  2=视频
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
        
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 创建时间
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
    public class classroom_subject_course_dto: classroom_subject_course
    {
        /// <summary>
        /// 是否完成
        /// </summary>
        public bool? IsComplete { get; set; }
    }
}
