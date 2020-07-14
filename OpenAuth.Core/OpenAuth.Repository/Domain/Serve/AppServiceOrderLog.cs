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
    [Table("appserviceorderlog")]
    public partial class AppServiceOrderLog : Entity
    {
        public AppServiceOrderLog()
        {
          this.Title= string.Empty;
          this.Details= string.Empty;
          this.CreateTime= DateTime.Now;
          this.CreateUserId= string.Empty;
          this.CreateUserName= string.Empty;
        }

        
        /// <summary>
        /// 头
        /// </summary>
        [Description("头")]
        public string Title { get; set; }
        /// <summary>
        /// 详细内容
        /// </summary>
        [Description("详细内容")]
        public string Details { get; set; }
        /// <summary>
        /// 时间
        /// </summary>
        [Description("时间")]
        public System.DateTime CreateTime { get; set; }
        /// <summary>
        /// 创建人Id
        /// </summary>
        [Description("创建人Id")]
        [Browsable(false)]
        public string CreateUserId { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        [Description("创建人")]
        public string CreateUserName { get; set; }
        /// <summary>
        /// 服务单Id
        /// </summary>
        [Description("服务单Id")]
        [Browsable(false)]
        public int? ServiceOrderId { get; set; }
        /// <summary>
        /// 工单Id
        /// </summary>
        [Description("工单Id")]
        public int? ServiceWorkOrder { get; set; }
    }
}