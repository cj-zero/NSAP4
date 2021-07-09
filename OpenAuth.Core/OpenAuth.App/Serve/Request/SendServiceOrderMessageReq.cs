using AutoMapper.Configuration.Annotations;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Request
{
    public class SendServiceOrderMessageReq
    {
        /// <summary>
        /// 服务单Id
        /// </summary>
        [Required]
        public int ServiceOrderId { get; set; }

        /// <summary>
        /// 技术员appUserId
        /// </summary>
        public int AppUserId { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 聊天信息id
        /// </summary>
        public string MessageId { get; set; }
        /// <summary>
        /// 撤回备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 服务单消息流水记录表
        /// </summary>
        [Ignore]
        public virtual List<ServiceOrderMessagePicture> ServiceOrderMessagePictures { get; set; }
    }
}
