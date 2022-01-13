using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Clue.ModelDto
{
    /// <summary>
    /// 跟进附件
    /// </summary>
    public class ClueFileUploadDto
    {
        public int Id { get; set; }
        /// <summary>
        /// 线索ID
        /// </summary>
        public int ClueId { get; set; }
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 地址或者是mimoID
        /// </summary>
        public string FileUrl { get; set; }
        /// <summary>
        /// 跟进id
        /// </summary>
        public int? ClueFollowUpId { get; set; }
        /// <summary>
        /// 文件类型
        /// </summary>
        public string FileType { get; set; }
    }
}
