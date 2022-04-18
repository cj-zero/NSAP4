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
    [Table("internalcontactmaterial")]
    public partial class InternalContactMaterial : Entity
    {
        public InternalContactMaterial()
        {
          this.InternalContactId= 0;
          this.MaterialCode= string.Empty;
          this.Prefix= string.Empty;
          this.Series= string.Empty;
          this.Special= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int InternalContactId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string MaterialCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Prefix { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Series { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? VoltsStart { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? VoltseEnd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? AmpsStart { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? AmpsEnd { get; set; }
        /// <summary>
        /// 电流单位
        /// </summary>
        [Description("")]
        public string CurrentUnit { get; set; }
        /// <summary>
        /// 夹具
        /// </summary>
        [Description("")]
        public string Fixture { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Special { get; set; }
        public DateTime? StartExcelTime { get; set; }
        public DateTime? EndExcelTime { get; set; }
        public string FromTheme { get; set; }
        public string FromThemeList { get; set; }
        public string SelectList { get; set; }
    }
}