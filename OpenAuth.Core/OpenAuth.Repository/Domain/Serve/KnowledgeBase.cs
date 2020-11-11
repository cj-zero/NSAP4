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
    [Table("knowledgebase")]
    public partial class KnowledgeBase : TreeEntity
    {
        public KnowledgeBase()
        {
          this.Org= string.Empty;
          this.Name= string.Empty;
          this.Content= string.Empty;
          this.ParentId= string.Empty;
          this.ParentName= string.Empty;
          this.CascadeId= string.Empty;
          this.CreateTime= DateTime.Now;
          this.CreateUserId= string.Empty;
          this.CreateUserName= string.Empty;
          this.UpdateTime= DateTime.Now;
          this.UpdateUserId= string.Empty;
          this.UpdateUserName= string.Empty;
        }

        
        /// <summary>
        /// 序号
        /// </summary>
        [Description("序号")]
        public int? SequenceNumber { get; set; }
        /// <summary>
        /// 类型（1 产品类型 2 问题类型 3 问题主题 4 问题现象 5 解决方案）
        /// </summary>
        [Description("类型")]
        public int? Type { get; set; }
        /// <summary>
        /// 提交信息部门
        /// </summary>
        [Description("提交信息部门")]
        public string Org { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        [Description("内容")]
        public string Content { get; set; }
        /// <summary>
        /// 编码
        /// </summary>
        [Description("编码")]
        public string Code { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CreateTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string CreateUserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CreateUserName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string UpdateUserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string UpdateUserName { get; set; }
    }
}