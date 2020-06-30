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
    [Table("AMR2")]
    public partial class AMR2 : Entity
    {
        public AMR2()
        {
          this.INMBaseRef= string.Empty;
          this.INMDocDate= DateTime.Now;
          this.ObjType= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Quantity { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Price { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? LineTotal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? RToStock { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? RActPrice { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? INMTransNm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? INMInst { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? INMTransTy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? INMCreatBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string INMBaseRef { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? INMDocDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? INMOpenQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ObjType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? LogInstanc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? IVLTransSe { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? IVLLayerID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? INMLineNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? INMSubLine { get; set; }
    }
}