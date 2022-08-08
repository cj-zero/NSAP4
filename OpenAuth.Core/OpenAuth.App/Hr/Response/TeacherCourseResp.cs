using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Hr
{
    /// <summary>
    /// 直播/回放 视频
    /// </summary>
    public  class TeacherCourseResp
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 视频标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 讲师名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// 使用人群
        /// </summary>
        public string ForTheCrowd { get; set; }
        /// <summary>
        /// 教学方式 1: 线下, 2：线上
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
        /// 观看次数
        /// </summary>
        public int ViewedCount { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime { get; set; }

        /// <summary>
        /// 开始时分
        /// </summary>
        public string StartHourMinute { get; set; }

        /// <summary>
        /// 结束时分
        /// </summary>
        public string EndHourMinute { get; set; }

        /// <summary>
        /// 已观看时长
        /// </summary>
        public int PlayDuration { get; set; }

    }

    /// <summary>
    /// 讲师开课状态标记
    /// </summary>
    public class teacher_course_sign : classroom_teacher_course
    {
        /// <summary>
        /// 排序值
        /// </summary>
        public int Sign { get; set; }
    }

}
