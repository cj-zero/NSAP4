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
	/// 解决方案
	/// </summary>
    [Table("solution")]
    public partial class Solution : Entity
    {
        public Solution()
        {
          this.SltCode= 0;
          this.Subject= string.Empty;
          this.Cause= string.Empty;
          this.Symptom= string.Empty;
          this.Descriptio= string.Empty;
          this.CreateUserId= string.Empty;
          this.CreateUserName= string.Empty;
          this.CreateTime= DateTime.Now;
          this.UpdateUserId= string.Empty;
          this.UpdateTime= DateTime.Now;
          this.UpdateUserName= string.Empty;
        }


        /// <summary>
        /// 编号
        /// </summary>
        [Description("编号")]
        public int SltCode { get; set; }
        /// <summary>
        /// 解决方案
        /// </summary>
        [Description("解决方案")]
        public string Subject { get; set; }
        /// <summary>
        /// 原因
        /// </summary>
        [Description("原因")]
        public string Cause { get; set; }
        /// <summary>
        /// 症状
        /// </summary>
        [Description("症状")]
        public string Symptom { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Description("备注")]
        public string Descriptio { get; set; }
        /// <summary>
        /// 状态（1发布，2检查，3内部）
        /// </summary>
        [Description("状态")]
        public int? Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string CreateUserId { get; set; }
        /// <summary>
        /// 创建人名字
        /// </summary>
        [Description("创建人名字")]
        public string CreateUserName { get; set; }
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
        public string UpdateUserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 更新人名字
        /// </summary>
        [Description("更新人名字")]
        public string UpdateUserName { get; set; }
    }
}