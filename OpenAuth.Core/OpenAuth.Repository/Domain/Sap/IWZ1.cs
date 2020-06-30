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
    [Table("IWZ1")]
    public partial class IWZ1 : Entity
    {
        public IWZ1()
        {
          this.ActName= string.Empty;
          this.ExecutLine= string.Empty;
          this.ActRevCncl= string.Empty;
          this.RevToAct= string.Empty;
          this.RvCaclDate= DateTime.Now;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ActName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? ActFrmBlnc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ExecutLine { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ActRevCncl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RevToAct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? ActDiffBal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? RvCaclDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? ErrReason { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? ActLastBal { get; set; }
    }
}