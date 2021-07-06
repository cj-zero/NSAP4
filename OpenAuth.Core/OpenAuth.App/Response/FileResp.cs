using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    public class FileResp
    {
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
    }
}
