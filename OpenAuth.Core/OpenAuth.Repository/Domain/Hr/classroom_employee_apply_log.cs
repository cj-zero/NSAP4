using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{

    /// <summary>
    /// 职工加入申请记录
    /// </summary>
    [Table("classroom_employee_apply_log")]
    public class classroom_employee_apply_log : BaseEntity<int>
    {
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
        /// 生日
        /// </summary>
        public DateTime Birthdate { get; set; }

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
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public string UpdateUser { get; set; }

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
