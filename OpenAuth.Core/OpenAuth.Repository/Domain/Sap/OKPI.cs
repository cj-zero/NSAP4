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
    [Table("OKPI")]
    public partial class OKPI : Entity
    {
        public OKPI()
        {
          this.Code= string.Empty;
          this.Name= string.Empty;
          this.Desc= string.Empty;
          this.ValueType= string.Empty;
          this.ViewName= string.Empty;
          this.ViewCtg= string.Empty;
          this.ViewSyn= string.Empty;
          this.TrendType= string.Empty;
          this.GoalQid= string.Empty;
          this.GoalDesc= string.Empty;
          this.CalcFrml= string.Empty;
          this.Visible= string.Empty;
          this.SBetter= string.Empty;
          this.Author= string.Empty;
          this.Version= string.Empty;
          this.CreateDate= DateTime.Now;
          this.IsSystem= string.Empty;
          this.MeasUnit= string.Empty;
          this.RevSign= string.Empty;
          this.Unit= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Code { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Desc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ValueType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? ValueQid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ViewName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ViewCtg { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ViewSyn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string TrendType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? TrendQid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? GoalValue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string GoalQid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string GoalDesc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CalcFrml { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Visible { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SBetter { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Author { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Version { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CreateDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? CreateTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IsSystem { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string MeasUnit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RevSign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Unit { get; set; }
    }
}