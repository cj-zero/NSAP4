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
    [Table("DATB")]
    public partial class DATB : Entity
    {
        public DATB()
        {
          this.TaxAcct= string.Empty;
          this.IsPLA= string.Empty;
          this.IsTaxCred= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? LocCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? NfTaxId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? TaxComId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? MatType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? ArchBal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string TaxAcct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IsPLA { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IsTaxCred { get; set; }
    }
}