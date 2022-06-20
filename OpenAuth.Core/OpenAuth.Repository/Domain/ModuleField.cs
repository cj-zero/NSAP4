using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 模块字段表
    /// </summary>
    [Table("ModuleField")]
    public partial class ModuleField : Entity
    {
        public ModuleField()
        {
            this.ModuleId = string.Empty;
            this.Key = string.Empty;
            this.Description = string.Empty;
            this.SortNo = 0;
        }

        /// <summary>
        /// 功能模块Id
        /// </summary>
        [Description("功能模块Id")]
        public string ModuleId { get; set; }
        /// <summary>
        /// 功能模块Code
        /// </summary>
        [Description("功能模块Code")]
        public string ModuleCode { get; set; }
        /// <summary>
        /// 字段名
        /// </summary>
        [Description("字段名")]
        public string Key { get; set; }
        /// <summary>
        /// 字段描述
        /// </summary>
        [Description("字段描述")]
        public string Description { get; set; }
        /// <summary>
        /// 排序号
        /// </summary>
        [Description("排序号")]
        public int? SortNo { get; set; }
        [Description("列表是否可见")]
        /// <summary>
        /// 列表是否可见
        /// </summary>
        public bool? IsList { get; set; }
        /// <summary>
        /// 表别名
        /// </summary>
        public string Alias { get; set; }
    }
}
