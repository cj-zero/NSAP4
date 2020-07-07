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
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
	/// 
	/// </summary>
    [Table("serviceordermessagepicture")]
    public partial class ServiceOrderMessagePicture : Entity
    {
        public ServiceOrderMessagePicture()
        {
          this.PictureId= string.Empty;
        }


        /// <summary>
        /// 消息通知流水Id
        /// </summary>
        [Description("消息通知流水Id")]
        public string ServiceOrderMessageId { get; set; }

        /// <summary>
        /// 图片Id
        /// </summary>
        [Description("图片Id")]
        public string PictureId { get; set; }
    }
}