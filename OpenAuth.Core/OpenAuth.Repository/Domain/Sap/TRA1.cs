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
    [Table("TRA1")]
    public partial class TRA1 : Entity
    {
        public TRA1()
        {
          this.PurPackMsr= string.Empty;
          this.ArsnalName= string.Empty;
          this.ArsnalCode= string.Empty;
          this.UnitMsr= string.Empty;
          this.ItemCode= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? BaseEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? PackQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PurPackMsr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ArsnalName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ArsnalCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string UnitMsr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Quantity { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Fraction { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Weight1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? LineTotal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ItemCode { get; set; }
    }
}