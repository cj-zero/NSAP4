using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    public class FileResp
    {
        public FileResp() 
        {
            AttachmentType = string.Empty;
        }
        /// <summary>
        /// 附件id
        /// </summary>
        public string FileId { get; set; }
        /// <summary>
        /// 附件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 附件类型
        /// </summary>
        public string FileType { get; set; }

        /// <summary>
        /// 附件类型（1.发票2.普通附件）
        /// </summary>
        public string AttachmentType { get; set; }
    }
}
