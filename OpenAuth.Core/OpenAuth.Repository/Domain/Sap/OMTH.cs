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
    [Table("OMTH")]
    public partial class OMTH : Entity
    {
        public OMTH()
        {
          this.IsCard= string.Empty;
          this.MatchType= string.Empty;
          this.MatchDate= DateTime.Now;
          this.CurrType= string.Empty;
          this.DataSource= string.Empty;
          this.createDate= DateTime.Now;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Totals { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IsCard { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string MatchType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? TransId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? MatchDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CurrType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DataSource { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? createDate { get; set; }
    }
}