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

namespace OpenAuth.Repository.Domain.Sap
{
    /// <summary>
	/// 
	/// </summary>
    [Table("WPK3")]
    public partial class WPK3 : Entity
    {
        public WPK3()
        {
          this.FldName= string.Empty;
          this.FldMethod= string.Empty;
          this.DbType= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? DsbrdEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FldName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FldMethod { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DbType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? SqlType { get; set; }
    }
}