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
    [Table("GBI10")]
    public partial class GBI10 : Entity
    {
        public GBI10()
        {
          this.LineTitle= string.Empty;
          this.LineNum= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string LineTitle { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string LineNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Amount { get; set; }
    }
}