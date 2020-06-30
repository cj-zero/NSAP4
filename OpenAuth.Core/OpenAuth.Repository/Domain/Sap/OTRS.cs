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
    [Table("OTRS")]
    public partial class OTRS : Entity
    {
        public OTRS()
        {
          this.ObjType= string.Empty;
          this.PeriodType= string.Empty;
          this.ApDate= DateTime.Now;
          this.DeclType= string.Empty;
          this.SCDateFrom= DateTime.Now;
          this.SCDateTo= DateTime.Now;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ObjType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? ReportType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PeriodType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? PeriodNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? Year { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? AdjustNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? ApUserSign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? ApDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? ApTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DeclType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? SCDateFrom { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? SCDateTo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? BosCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? BatchNum { get; set; }
    }
}