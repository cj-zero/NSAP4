using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Mail
{
    public class MailRequest
    {
        /// <summary>
        /// 主题
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// 邮件紧急程度 0 非紧急，1 正常，2 紧急
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// 发件人
        /// </summary>
        public MailUser FromUser;
        /// <summary>
        /// 收件人
        /// </summary>
        public List<MailUser> ToUsers;
        /// <summary>
        /// 抄送人
        /// </summary>
        public List<MailUser> CcUsers;
        /// <summary>
        /// 附件 
        /// </summary>
        public List<MailAttachment> Attachments;
        /// <summary>
        /// 邮件内容
        /// </summary>
        public List<MailContent> Contents;
    }
    public class MailUser
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 邮件地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 邮箱密码
        /// </summary>
        public string Password { get; set; }
    }
    public class MailAttachment
    {
        /// <summary>
        /// 附件地址
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// 附件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 附件类型 例如：application/pdf
        /// </summary>
        public string FileType { get; set; }
    }

    public class MailContent 
    {
        /// <summary>
        /// 内容类型 例如：plain，html
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
    }
}
