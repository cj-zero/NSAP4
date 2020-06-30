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
    [Table("WTQ9")]
    public partial class WTQ9 : Entity
    {
        public WTQ9()
        {
          this.TargetBase= string.Empty;
          this.ObjType= string.Empty;
          this.BsDocDate= DateTime.Now;
          this.BsDueDate= DateTime.Now;
          this.BsCardName= string.Empty;
          this.BsComments= string.Empty;
          this.Posted= string.Empty;
          this.IsGross= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? BaseAbs { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? BaseLine { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string TargetBase { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ObjType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? DrawnSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? DrawnSumFc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? DrawnSumSc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? ApplDrawn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? ApplDrawnF { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? ApplDrawnS { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? BaseDocNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? BsDocDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? BsDueDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BsCardName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BsComments { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Posted { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Vat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? VatFc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? VatSc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Gross { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? GrossFc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? GrossSc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IsGross { get; set; }
    }
}