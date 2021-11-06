using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Serve
{
    /// <summary>
    /// 附件
    /// </summary>
    [Table("meetingfile")]
    public class MeetingFile : BaseEntity<int>
    {
        /// <summary>
        /// 会议Id
        /// </summary>
        [Description("会议Id")]
        public int MeetingId { get; set; }
        /// <summary>
        /// 文件地址
        /// </summary>
        [Description("文件地址")]
        public string FileUrl { get; set; }
        /// <summary>
        /// 文件名
        /// </summary>
        [Description("文件名")]
        public string Name { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        [Description("类型")]
        public string Type { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Description("备注")]
        public string Remake { get; set; }
        /// <summary>
        /// 上传时间
        /// </summary>
        [Description("上传时间")]
        public DateTime UploadTime { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        [Description("创建人")]
        public string CreateUser { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        [Description("是否删除")]
        public bool IsDelete { get; set; } = false;
        public override void GenerateDefaultKeyVal()
        {

        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
