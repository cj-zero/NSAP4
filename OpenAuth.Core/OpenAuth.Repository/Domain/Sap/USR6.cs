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
    [Table("USR6")]
    public partial class USR6 : Entity
    {
        public USR6()
        {
          this.DigCrtPath= string.Empty;
          this.AcsDsbldBP= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DigCrtPath { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AcsDsbldBP { get; set; }
    }
}