using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace OpenAuth.App.Order.Request
{
    /// <summary>
    /// 单据附件
    /// </summary>
    [Serializable]
    [DataContract]
    public class billAttchment
    {
        /// <summary>
        /// 附件ID
        /// </summary>
        [DataMember]
        public string fileId { get; set; }
        /// <summary>
        /// 附件类型
        /// </summary>
        [DataMember]
        public string filetype { get; set; }
        /// <summary>
        /// 附件类型Id
        /// </summary>
        [DataMember]
        public string filetypeId { get; set; }
        /// <summary>
        /// 附件名称
        /// </summary>
        [DataMember]
        public string filename { get; set; }
        /// <summary>
        /// 附件名称
        /// </summary>
        [DataMember]
        public string realName { get; set; }
        /// <summary>
        /// 附件备注
        /// </summary>
        [DataMember]
        public string remarks { get; set; }
        /// <summary>
        /// 附件下载路径
        /// </summary>
        [DataMember]
        public string filepath { get; set; }
        /// <summary>
        /// 附件预览路径
        /// </summary>
        [DataMember]
        public string attachPath { get; set; }
        /// <summary>
        /// 上传时间
        /// </summary>
        [DataMember]
        public string filetime { get; set; }
        /// <summary>
        /// 操作者
        /// </summary>
        [DataMember]
        public string username { get; set; }
        /// <summary>
        /// 用户Id
        /// </summary>
        [DataMember]
        public string fileUserId { get; set; }
    }
}
