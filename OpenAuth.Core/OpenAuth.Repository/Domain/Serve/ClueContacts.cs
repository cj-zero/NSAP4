using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Serve
{
    /// <summary>
    /// 线索联系人
    /// </summary>
    [Table("cluecontacts")]
    public class ClueContacts : BaseEntity<int>
    {
        /// <summary>
        /// 线索ID
        /// </summary>
        [Description("线索ID")]
        public int ClueId { get; set; }
        /// <summary>
        /// 联系人名称
        /// </summary>
        [Description("联系人名称")]
        public string Name { get; set; }
        /// <summary>
        /// 联系方式1
        /// </summary>
        [Description("联系方式1")]
        public string Tel1 { get; set; }
        /// <summary>
        /// 联系方式2
        /// </summary>
        [Description("联系方式2")]
        public string Tel2 { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 角色（0：决策者、1：普通人）
        /// </summary>
        [Description("角色")]
        public int Role { get; set; }
        /// <summary>
        ///职位
        /// </summary>
        [Description("职位")]
        public string Position { get; set; }
        /// <summary>
        /// 详细地址（地址（省市））
        /// </summary>
        [Description("地址（省市）")]
        public string Address1 { get; set; }
        /// <summary>
        /// 详细地址
        /// </summary>
        [Description("详细地址")] 
        public string Address2 { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        [Description("创建人")]
        public string CreateUser { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public System.DateTime CreateTime { get; set; }
        /// <summary>
        /// 更新人
        /// </summary>
        [Description("更新人")]
        public string UpdateUser { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        [Description("更新时间")]
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        [Description("是否删除")]
        public bool IsDelete { get; set; } = false;
        /// <summary>
        /// 是否默认
        /// </summary>
        [Description("是否默认")]
        public bool IsDefault { get; set; } 

        public override void GenerateDefaultKeyVal()
        {

        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
