using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Mail
{
    /// <summary>
    /// 邮件帮助类
    /// </summary>
    public class MailHelper
    {
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task Sendmail(MailRequest request)
        {
            var messageToSend = new MimeMessage();
            messageToSend.Subject = request.Subject;
            List<StreamReader> streamReaders = new List<StreamReader>();
            List<MimePart> mimeParts = new List<MimePart>();
            List<TextPart> textParts = new List<TextPart>();
            if (request.Contents.Count > 0)
            {
                request.Contents.ForEach(c =>
                {
                    textParts.Add(new TextPart(c.Type) { Text = c.Content });
                });
            }
            if (request.FromUser != null)
            {
                messageToSend.From.Add(new MailboxAddress(request.FromUser.Name, request.FromUser.Address));
            }
            if (request.ToUsers.Count > 0)
            {
                request.ToUsers.ForEach(t => messageToSend.To.Add(new MailboxAddress(t.Name, t.Address)));
            }
            if (request.CcUsers.Count > 0)
            {
                request.CcUsers.ForEach(c => messageToSend.Cc.Add(new MailboxAddress(c.Name, c.Address)));
            }
            if (request.Attachments.Count > 0) 
            {
                request.Attachments.ForEach(a =>
                {
                    StreamReader sr = new StreamReader(a.FilePath);
                    var mimePart =new MimePart(a.FileType) { ContentTransferEncoding = ContentEncoding.Base64, ContentDisposition = new ContentDisposition(ContentDisposition.Attachment), Content = new MimeContent(sr.BaseStream) };
                    mimePart.ContentDisposition.Parameters.Add(new Parameter("GB2312", "filename", a.FileName) { EncodingMethod = ParameterEncodingMethod.Rfc2047 });
                    mimeParts.Add(mimePart);
                    streamReaders.Add(sr);
                });
            }
            var multipart = new Multipart("mixed");
            textParts.ForEach(t => multipart.Add(t));
            mimeParts.ForEach(m => multipart.Add(m));
            messageToSend.Body = multipart;
            switch (request.Priority)
            {
                case 0:
                    messageToSend.Priority = MessagePriority.NonUrgent;
                    break;
                case 1:
                    messageToSend.Priority = MessagePriority.Normal;
                    break;
                case 2:
                    messageToSend.Priority = MessagePriority.Urgent;
                    break;
                default:
                    break;
            }
            using (var smtp = new SmtpClient())
            {
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
                //await smtp.ConnectAsync("mail.neware.com.cn", 25, SecureSocketOptions.Auto);
                await smtp.ConnectAsync("smtphz.qiye.163.com", 25, SecureSocketOptions.Auto);
                await smtp.AuthenticateAsync(request.FromUser.Address, request.FromUser.Password);
                await smtp.SendAsync(messageToSend);
                await smtp.DisconnectAsync(true);
            }
            if (streamReaders.Count > 0) 
            {
                streamReaders.ForEach(s => { s.Close(); s.Dispose(); });
            }
        }
    }
}
