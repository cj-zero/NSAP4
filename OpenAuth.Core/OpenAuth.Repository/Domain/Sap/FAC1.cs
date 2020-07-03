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
    [Table("FAC1")]
    public partial class FAC1 : Entity
    {
        public FAC1()
        {
          this.DprArea= string.Empty;
          this.TransType= string.Empty;
          this.OldDprType= string.Empty;
          this.NewDprType= string.Empty;
          this.OldDprDate= DateTime.Now;
          this.NewDprDate= DateTime.Now;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DprArea { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string TransType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string OldDprType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string NewDprType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? OldUsfLife { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? NewUsfLife { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? OldDprDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? NewDprDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? OldSalVal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? NewSalVal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? OldTtlUnit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? NewTtlUnit { get; set; }
    }
}