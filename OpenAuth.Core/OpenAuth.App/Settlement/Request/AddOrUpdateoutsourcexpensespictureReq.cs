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
using Infrastructure.AutoMapper;
using OpenAuth.Repository.Core;
using OpenAuth.Repository.Domain.Settlement;

namespace OpenAuth.App.Request
{
    /// <summary>
	/// 
	/// </summary>
    [Table("outsourcexpensespicture")]
    [AutoMapTo(typeof(outsourcexpensespicture))]
    public partial class AddOrUpdateoutsourcexpensespictureReq 
    {

        /// <summary>
        /// 
        /// </summary>
        public string PictureId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string outsourcExpensesId { get; set; }

        /// <summary>
        /// 附件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 附件类型
        /// </summary>
        public string FileType { get; set; }

        //todo:添加自己的请求字段
    }
}