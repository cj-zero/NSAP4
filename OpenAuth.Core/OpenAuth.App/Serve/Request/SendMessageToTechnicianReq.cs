using AutoMapper.Configuration.Annotations;
using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    [AutoMapTo(typeof(ServiceOrderMessage), false)]
    public class SendMessageToTechnicianReq
    {
        /// <summary>
        /// 工单对应技术员名称
        /// </summary>
        public string FroTechnicianName { get; set; }
        /// <summary>
        /// 技术员Id
        /// </summary>
        public string FroTechnicianId { get; set; }
        /// <summary>
        /// 服务单Id
        /// </summary>
        public int? ServiceOrderId { get; set; }
        ///<summary>
        /// 消息内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// app用户Id
        /// </summary>
        public int AppUserId { get; set; }

        /// <summary>
        /// 催办
        /// </summary>
        public int MessageType { get; set; }

        /// <summary>
        /// 服务单消息流水记录表
        /// </summary>
        [Ignore]
        public virtual List<ServiceOrderMessagePicture> ServiceOrderMessagePictures { get; set; }
    }
}
