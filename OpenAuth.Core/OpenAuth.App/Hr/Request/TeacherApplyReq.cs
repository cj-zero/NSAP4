using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Hr
{
    /// <summary>
    /// 讲师申请
    /// </summary>
    public class TeacherApplyReq
    {
        /// <summary>
        /// 名字
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 年龄
        /// </summary>
        public int Age { get; set; }
        /// <summary>
        /// 手机
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        public string HeaderImg { get; set; }
        /// <summary>
        /// 部门
        /// </summary>
        public string Department { get; set; }
        /// <summary>
        /// 课授课程
        /// </summary>
        public string CanTeachCourse { get; set; }
        /// <summary>
        /// 擅长领域
        /// </summary>
        public string BeGoodATTerritory { get; set; }
        /// <summary>
        /// 用户Id
        /// </summary>
        public int AppUserId { get; set; }

    }
}
