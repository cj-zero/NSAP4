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
    [Table("OOCR")]
    public partial class OOCR : Entity
    {
        public OOCR()
        {
          this.OcrName= string.Empty;
          this.Direct= string.Empty;
          this.Locked= string.Empty;
          this.DataSource= string.Empty;
          this.Active= string.Empty;
          this.updateDate= DateTime.Now;
          this.IsFixedAmt= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string OcrName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? OcrTotal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Direct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Locked { get; set; }
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
        public short? DimCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? AbsEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Active { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? logInstanc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? updateDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IsFixedAmt { get; set; }
    }
}