using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Clue.ModelDto
{
    /// <summary>
    /// 联系人model
    /// </summary>
    public class ClueContactsListDto
    {
        public int Id { get; set; }
        /// <summary>
        /// 线索ID
        /// </summary>
        public int ClueId { get; set; }
        /// <summary>
        /// 联系人名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 联系方式1
        /// </summary>
        public string Tel1 { get; set; }
        /// <summary>
        /// 联系方式2
        /// </summary>
        public string Tel2 { get; set; }
        /// <summary>
        /// 角色（0：决策者、1：普通人）
        /// </summary>
        public int Role { get; set; }
        /// <summary>
        ///职位
        /// </summary>
        public string Position { get; set; }
        /// <summary>
        /// 详细地址（地址（省市））
        /// </summary>
        public string Address1 { get; set; }
        /// <summary>
        /// 详细地址
        /// </summary>
        public string Address2 { get; set; }
        /// <summary>
        /// 是否默认
        /// </summary>
        public bool IsDefault { get; set; }
    }
}
