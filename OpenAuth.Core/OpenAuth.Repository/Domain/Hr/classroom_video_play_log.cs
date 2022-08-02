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
    [Table("classroom_video_play_log")]
    public class classroom_video_play_log : BaseEntity<long>
    {
        /// <summary>
        /// 课程包id
        /// </summary>
        public int CoursePackageId { get; set; }
        /// <summary>
        /// 课程id
        /// </summary>
        public int CourseId { get; set; }
        /// <summary>
        /// 课程视频id
        /// </summary>
        public int CourseVideoId { get; set; }
        /// <summary>
        /// 用户id
        /// </summary>
        public int AppUserId { get; set; }
        /// <summary>
        /// 播放时长
        /// </summary>
        public int PlayDuration { get; set; }
        /// <summary>
        /// 视频总时长
        /// </summary>
        public int TotalDuration { get; set; }
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
}
