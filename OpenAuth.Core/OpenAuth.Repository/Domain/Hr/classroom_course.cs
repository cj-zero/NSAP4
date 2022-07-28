using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 课程表
    /// </summary>
    [Table("classroom_course")]
    public class classroom_course : BaseEntity<int>
    {
        /// <summary>
        /// 课程名字
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 课程来源(1:主管推课  2:职前  3:入职  4:晋升  5:转正 6:变动)
        /// </summary>
        public int Source { get; set; }
        /// <summary>
        /// 课程学习周期(天)
        /// </summary>
        public int LearningCycle { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 课程开放状态
        /// </summary>
        public bool State { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }

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
