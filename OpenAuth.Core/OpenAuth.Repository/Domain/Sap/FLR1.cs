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
    [Table("FLR1")]
    public partial class FLR1 : Entity
    {
        public FLR1()
        {
          this.CompValue= string.Empty;
          this.CompOper= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? LineNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CompValue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CompOper { get; set; }
    }
}