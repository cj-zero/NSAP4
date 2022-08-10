using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{

    /// <summary>
    ///  讲师课程播放记录
    /// </summary>
    [Table("classroom_teacher_course_play_log")]
    public class classroom_teacher_course_play_log : BaseEntity<int>
    {
        /// <summary>
        /// 讲师视频Id
        /// </summary>
        [Description("讲师视频Id")]
        public int TeacherCourseId { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        [Description("用户id")]
        public int AppUserId { get; set; }
        /// <summary>
        /// 播放时长
        /// </summary>
        [Description("播放时长")]
        public int PlayDuration { get; set; }
        /// <summary>
        /// 视频总时长
        /// </summary>
        [Description("视频总时长")]
        public int TotalDuration { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Description("更新时间")]
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        [Description("更新时间")]
        public int? UpdateUser{ get; set; }

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
