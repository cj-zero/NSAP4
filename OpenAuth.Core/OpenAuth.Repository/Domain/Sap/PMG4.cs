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
    [Table("PMG4")]
    public partial class PMG4 : Entity
    {
        public PMG4()
        {
          this.DocDate= DateTime.Now;
          this.Status= string.Empty;
          this.AmountCat= string.Empty;
          this.Categorize= string.Empty;
          this.Operation= string.Empty;
          this.Chargeable= string.Empty;
          this.DocType= string.Empty;
          this.EncryptIV= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? StageID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? TYP { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? DOCNUM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? DocEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? DocDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Total { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? LineNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? LogInstanc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AmountCat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Categorize { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Operation { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Chargeable { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Charged { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? ChargedQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DocType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EncryptIV { get; set; }
    }
}