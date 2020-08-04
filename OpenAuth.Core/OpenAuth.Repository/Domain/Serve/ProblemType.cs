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
	/// 问题类型表
	/// </summary>
    [Table("problemtype")]
    public partial class ProblemType : Entity
    {
        public ProblemType()
        {
            this.Name = string.Empty;
            this.Description = string.Empty;
            this.ParentId = string.Empty;
            this.PrblmTypID = 0;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("问题名称")]
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("问题描述")]
        public string Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("状态")]
        public ushort? InuseFlag { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("上一级")]
        [Browsable(false)]
        public string ParentId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("显示顺序")]
        [Browsable(false)]
        public int OrderIdx { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Description("问题类型SAP ID")]
        [Browsable(false)]
        public int PrblmTypID { get; set; }
    }
}