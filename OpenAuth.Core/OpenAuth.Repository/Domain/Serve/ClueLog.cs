using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Serve
{
    /// <summary>
    /// 线索操作日志
    /// </summary>
    [Table("cluelog")]
    public class ClueLog : BaseEntity<int>
    {
        /// <summary>
        /// 线索ID
        /// </summary>
        [Description("线索ID")]
        public int ClueId { get; set; }
        /// <summary>
        /// 操作类型（0：新增，1：编辑，2：删除）
        /// </summary>
        [Description("操作类型（0：新增，1：编辑，2：删除）")]
        public int LogType { get; set; }
        /// <summary>
        /// 操作内容
        /// </summary>
        [Description("操作内容")]
        public string Details { get; set; }
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


        public override void GenerateDefaultKeyVal()
        {

        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
