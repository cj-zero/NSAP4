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
    [Table("OHLD")]
    public partial class OHLD : Entity
    {
        public OHLD()
        {
          this.WndFrm= string.Empty;
          this.WndTo= string.Empty;
          this.isCurYear= string.Empty;
          this.ignrWnd= string.Empty;
          this.WeekNoRule= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string WndFrm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string WndTo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string isCurYear { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ignrWnd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string WeekNoRule { get; set; }
    }
}