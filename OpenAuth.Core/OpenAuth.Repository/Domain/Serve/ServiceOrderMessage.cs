﻿//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a CodeSmith Template.
//
//     DO NOT MODIFY contents of this file. Changes to this
//     file will be lost if the code is regenerated.
//     Author:Yubao Li
// </autogenerated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using AutoMapper.Configuration.Annotations;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
	/// 服务单消息流水记录表
	/// </summary>
    [Table("serviceordermessage")]
    public partial class ServiceOrderMessage : Entity
    {
        public ServiceOrderMessage()
        {
            this.FroTechnicianName = string.Empty;
            this.FroTechnicianId = string.Empty;
            this.Content = string.Empty;
            this.Replier = string.Empty;
            this.ReplierId = string.Empty;
            this.CreateTime = DateTime.Now;
            this.MessageType = 0;
        }


        /// <summary>
        /// 服务单Id
        /// </summary>
        [Description("服务单Id")]
        //[Browsable(false)]
        public int? ServiceOrderId { get; set; }
        /// <summary>
        /// 工单Id
        /// </summary>
        [Description("工单Id")]
        //[Browsable(false)]
        public int? ServiceWorkOrderId { get; set; }
        /// <summary>
        /// 工单对应技术员名称
        /// </summary>
        [Description("工单对应技术员名称")]
        public string FroTechnicianName { get; set; }
        /// <summary>
        /// 技术员Id
        /// </summary>
        [Description("技术员Id")]
        [Browsable(false)]
        public string FroTechnicianId { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        [Description("消息内容")]
        public string Content { get; set; }
        /// <summary>
        /// 回复人名字
        /// </summary>
        [Description("回复人名字")]
        public string Replier { get; set; }
        /// <summary>
        /// 回复人Id
        /// </summary>
        [Description("回复人Id")]
        [Browsable(false)]
        public string ReplierId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public System.DateTime? CreateTime { get; set; }

        /// <summary>
        /// app用户Id
        /// </summary>
        public int AppUserId { get; set; }

        /// <summary>
        /// 催办
        /// </summary>
        public int? MessageType { get; set; }

        /// <summary>
        /// 服务单消息流水记录表
        /// </summary>
        [Ignore]
        public virtual List<ServiceOrderMessagePicture> ServiceOrderMessagePictures { get; set; }


    }
}