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
    [Table("IWZ3")]
    public partial class IWZ3 : Entity
    {
        public IWZ3()
        {
          this.BaseCurr= string.Empty;
          this.RevalDate= DateTime.Now;
          this.RealAcct= string.Empty;
          this.RevalOfsac= string.Empty;
          this.RevalType= string.Empty;
          this.ExeLine= string.Empty;
          this.RevCancel= string.Empty;
          this.RvCaclDate= DateTime.Now;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? RevalPrice { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BaseCurr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? RevalDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? RevalSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? BasePrice { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RealAcct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RevalOfsac { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RevalType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ExeLine { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RevCancel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? RvCaclDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Quantity { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? BalanceBef { get; set; }
    }
}