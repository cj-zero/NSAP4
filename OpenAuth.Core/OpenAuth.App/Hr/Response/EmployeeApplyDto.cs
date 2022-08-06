using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Hr
{
    /// <summary>
    /// 
    /// </summary>
    public class EmployeeApplyDto
    {
        /// <summary>
        ///  记录id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 性别 1:男 2:女
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// 年龄
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// App用户id
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

        /// <summary>
        /// 审核状态(1:未审核 2:审核已通过 3:已驳回 4.封禁)
        /// </summary>
        public int AuditState { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; }
    }
}
