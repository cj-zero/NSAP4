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
	/// 返厂维修附件及图片
	/// </summary>
    [Table("expresspicture")]
    public partial class ExpressPicture : Entity
    {
        public ExpressPicture()
        {
            this.ExpressId = string.Empty;
            this.PictureId = string.Empty;
        }


        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string ExpressId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string PictureId { get; set; }
    }
}