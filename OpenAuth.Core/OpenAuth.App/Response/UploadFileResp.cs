using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    [AutoMapTo(typeof(UploadFile))]
    public class UploadFileResp
    {
        public string Id { get; set; }
        /// <summary>
	    /// 文件名称
	    /// </summary>
        public string FileName { get; set; }
        /// <summary>
	    /// 文件路径
	    /// </summary>
        public string FilePath { get; set; }
        /// <summary>
	    /// 描述
	    /// </summary>
        public string Description { get; set; }
        /// <summary>
	    /// 文件类型
	    /// </summary>
        public string FileType { get; set; }
        /// <summary>
	    /// 文件大小
	    /// </summary>
        public long FileSize { get; set; }
        /// <summary>
	    /// 上传人姓名
	    /// </summary>
        public string CreateUserName { get; set; }

        /// <summary>
        /// 图片类型
        /// </summary>
        public int? PictureType { get; set; }
    }
}
