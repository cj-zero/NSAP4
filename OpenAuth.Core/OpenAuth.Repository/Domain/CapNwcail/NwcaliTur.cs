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

namespace OpenAuth.Repository.Domain.CapNwcail
{
    /// <summary>
	/// 
	/// </summary>
    [Table("nwcalitur")]
    public partial class NwcaliTur : Entity
    {
        public NwcaliTur()
        {
          this.NwcaliBaseInfoId = string.Empty;
          this.UncertaintyContributors= string.Empty;
          this.Unit= string.Empty;
          this.Type= string.Empty;
          this.Distribution= string.Empty;
          this.DegreesOfFreedom= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string NwcaliBaseInfoId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? DataType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public double? Range { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public double? TestPoint { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public double? Tur { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string UncertaintyContributors { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public double? SensitivityCoefficient { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public double Value { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Unit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Distribution { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public double? Divisor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public double StdUncertainty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DegreesOfFreedom { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public double? SignificanceCheck { get; set; }
    }
}