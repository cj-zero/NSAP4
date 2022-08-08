using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace OpenAuth.App.Hr
{
    /// <summary>
    /// 教学视频标识
    /// </summary>
    public enum TeacherCourseSignEnum
    {
        /// <summary>
        /// 默认
        /// </summary>
        [Description("无")]
        Default = 0,

        /// <summary>
        /// 线下已结束
        /// </summary>
        [Description("线下已结束")]
        EndOffline = 1,

        /// <summary>
        /// 线上待录播
        /// </summary>
        [Description("线上待录播")]
        EndOnLine = 2,

        /// <summary>
        /// 预告
        /// </summary>
        [Description("预告")]
        AdvanceNotice = 3,

        /// <summary>
        /// 开课中
        /// </summary>
        [Description("开课中")]
        InClass = 4,

        /// <summary>
        ///  直播中
        /// </summary>
        [Description("直播中")]
        Living = 5,

    }
}
