using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 新威课堂讲师申请表
    /// </summary>
    [Table("classroom_teacher_apply_log")]
    public partial class classroom_teacher_apply_log : BaseEntity<int>
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
        /// 手机号码
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 审核状态(1:未审核 2:审核已通过 3:已驳回)
        /// </summary>
        public int AuditState { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        public string HeaderImg { get; set; }
        /// <summary>
        /// 部门
        /// </summary>
        public string Department { get; set; }
        /// <summary>
        /// 可授课程
        /// </summary>
        public string CanTeachCourse { get; set; }
        /// <summary>
        /// 擅长领域
        /// </summary>
        public string BeGoodATTerritory { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifyTime { get; set; }
        /// <summary>
        /// 操作人
        /// </summary>
        public string OperationUser { get; set; }
        /// <summary>
        /// App用户id
        /// </summary>
        public int AppUserId { get; set; }
        /// <summary>
        /// 等级
        /// </summary>
        public int Grade { get; set; }
        /// <summary>
        /// 经验
        /// </summary>
        public int Experience { get; set; }
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
