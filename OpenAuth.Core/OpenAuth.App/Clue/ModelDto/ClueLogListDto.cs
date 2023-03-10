using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Clue.ModelDto
{
    /// <summary>
    /// 操作日志Dto
    /// </summary>
    public class ClueLogListDto
    {
        public int Id { get; set; }
        /// <summary>
        /// 线索ID
        /// </summary>
        public int ClueId { get; set; }
        /// <summary>
        /// 操作类型（0：新增，1：编辑，2：删除）
        /// </summary>
        public string LogType { get; set; }
        /// <summary>
        /// 操作内容
        /// </summary>
        public string Details { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
