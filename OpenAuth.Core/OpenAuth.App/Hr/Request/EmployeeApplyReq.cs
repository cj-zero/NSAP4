using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Hr
{
    /// <summary>
    /// 员工加入申请
    /// </summary>
    public class EmployeeApplyReq
    {
        /// <summary>
        /// 名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 手机
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 性别 1:男 2:女
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        public DateTime Birthdate { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public int AppUserId { get; set; }

        /// <summary>
        /// 毕业学校
        /// </summary>
        public string GraduationSchool { get; set; }


        /// <summary>
        /// 简历文件名
        /// </summary>
        public string ResumeFileName { get; set; }

        /// <summary>
        /// 简历文件地址
        /// </summary>
        public string ResumeFilePath { get; set; }
    }
}
