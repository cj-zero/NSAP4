using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Meeting.Request
{
    public class FileReq
    {
        /// <summary>
        /// 文件地址
        /// </summary>
        public string FileUrl { get; set; }
        /// <summary>
        /// 文件名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remake { get; set; }
        /// <summary>
        /// 上传时间
        /// </summary>
        public DateTime UploadTime { get; set; }
    }
}
