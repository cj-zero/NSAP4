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
    [Table("TPW3")]
    public partial class TPW3 : Entity
    {
        public TPW3()
        {
          this.PymMeth= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? CrUtiliz { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? CrUtilizFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? CrUtilizSC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PymMeth { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? PymAmnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? PymAmntFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? PymAmntSC { get; set; }
    }
}