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
    [Table("RTM1")]
    public partial class RTM1 : Entity
    {
        public RTM1()
        {
          this.AcctCode= string.Empty;
          this.Valid= string.Empty;
          this.FCCurrency= string.Empty;
          this.JdtAcctCod= string.Empty;
          this.BaseRef= string.Empty;
          this.JdtType= string.Empty;
          this.RefDate= DateTime.Now;
          this.DueDate= DateTime.Now;
          this.TaxDate= DateTime.Now;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AcctCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Balance { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? FrnBlnc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Valid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? RevalRate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Delta { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FCCurrency { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string JdtAcctCod { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BaseRef { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string JdtType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? RefDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? DueDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? TaxDate { get; set; }
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
        public decimal? CntBlnc { get; set; }
    }
}