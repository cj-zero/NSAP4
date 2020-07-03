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
    [Table("OBOT")]
    public partial class OBOT : Entity
    {
        public OBOT()
        {
          this.StatusFrom= string.Empty;
          this.StatusTo= string.Empty;
          this.TranDate= DateTime.Now;
          this.Reconciled= string.Empty;
          this.PostDate= DateTime.Now;
          this.TaxDate= DateTime.Now;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string StatusFrom { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string StatusTo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? TranDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? TranTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Reconciled { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? TransId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? PostDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? TaxDate { get; set; }
    }
}