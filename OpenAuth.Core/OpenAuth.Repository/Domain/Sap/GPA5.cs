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
    [Table("GPA5")]
    public partial class GPA5 : Entity
    {
        public GPA5()
        {
          this.Selected= string.Empty;
          this.LnType= string.Empty;
          this.ProdCode= string.Empty;
          this.ProdDescr= string.Empty;
          this.DocType= string.Empty;
          this.PostDate= DateTime.Now;
          this.ItemCode= string.Empty;
          this.ItemDescr= string.Empty;
          this.DistNumber= string.Empty;
          this.DebCred= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Selected { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string LnType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ProdCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ProdDescr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? PODocAbs { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? PODocNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DocType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? DocAbs { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? DocNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? DocLineNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? PostDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ItemCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ItemDescr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DistNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? SnBAbs { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? ManagedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? LnQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? SnBQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? LnCost { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DebCred { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? LnTotal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? SnBTotal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? SBAccTotal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? SBAccQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? ACAccTotal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? ACAccQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Applied { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Variance { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? MRVAdjust { get; set; }
    }
}