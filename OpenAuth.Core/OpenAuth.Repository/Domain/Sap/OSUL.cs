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
    [Table("OSUL")]
    public partial class OSUL : Entity
    {
        public OSUL()
        {
          this.RealName= string.Empty;
          this.LogReason= string.Empty;
          this.LogDetail= string.Empty;
          this.Mac= string.Empty;
          this.Machine= string.Empty;
          this.StartDate= DateTime.Now;
          this.EndDate= DateTime.Now;
          this.ChkHash= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RealName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string LogReason { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string LogDetail { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Mac { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Machine { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? StartDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? StartTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? EndDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? EndTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ChkHash { get; set; }
    }
}