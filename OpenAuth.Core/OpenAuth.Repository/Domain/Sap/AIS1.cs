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
    [Table("AIS1")]
    public partial class AIS1 : Entity
    {
        public AIS1()
        {
          this.SrcLocName= string.Empty;
          this.SrcTaxAcct= string.Empty;
          this.SrcSubType= string.Empty;
          this.ITCType= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? SourceType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? SourceNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? SrcEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? SrcLocCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SrcLocName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? SrcStaType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SrcTaxAcct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? SACEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? CrdtAmnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? DistAmnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SrcSubType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ITCType { get; set; }
    }
}