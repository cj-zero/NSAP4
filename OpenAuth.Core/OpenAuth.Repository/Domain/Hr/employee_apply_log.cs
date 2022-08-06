using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Hr
{

    /// <summary>
    /// 职工加入申请记录
    /// </summary>
    [Table("employee_apply_log")]
    public class employee_apply_log : BaseEntity<int>
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
        /// 生日
        /// </summary>
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// App用户id
        /// </summary>
        public int AppUserId { get; set; }

        /// <summary>
        /// 毕业学校
        /// </summary>
        public string GraduationSchool { get; set; }

        /// <summary>
        /// 简历地址
        /// </summary>
        public string ResumeUrl { get; set; }

        /// <summary>
        /// 审核状态(1:未审核 2:审核已通过 3:已驳回 4.封禁)
        /// </summary>
        public int AuditState { get; set; }

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
