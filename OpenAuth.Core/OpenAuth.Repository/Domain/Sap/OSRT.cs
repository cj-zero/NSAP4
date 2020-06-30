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
    [Table("OSRT")]
    public partial class OSRT : Entity
    {
        public OSRT()
        {
          this.SumRptId= string.Empty;
          this.SumRptDate= DateTime.Now;
          this.FromDate= DateTime.Now;
          this.ToDate= DateTime.Now;
          this.RptType= string.Empty;
          this.version= string.Empty;
          this.SumSubId= string.Empty;
          this.SumRptType= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string SumRptId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? SumRptDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? BPLId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? FromDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? ToDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RptType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string version { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string SumSubId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SumRptType { get; set; }
    }
}