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
    [Table("RPC21")]
    public partial class RPC21 : Entity
    {
        public RPC21()
        {
          this.ObjectType= string.Empty;
          this.ExtDocNum= string.Empty;
          this.RefObjType= string.Empty;
          this.AccessKey= string.Empty;
          this.IssueDate= DateTime.Now;
          this.IssuerCNPJ= string.Empty;
          this.IssuerCode= string.Empty;
          this.Model= string.Empty;
          this.Series= string.Empty;
          this.RefAccKey= string.Empty;
          this.SubSeries= string.Empty;
          this.Remark= string.Empty;
          this.LinkRefTyp= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ObjectType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? LogInstanc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? RefDocEntr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? RefDocNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ExtDocNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RefObjType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AccessKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? IssueDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IssuerCNPJ { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IssuerCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Model { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Series { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? Number { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RefAccKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? RefAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SubSeries { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Remark { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string LinkRefTyp { get; set; }
    }
}