//------------------------------------------------------------------------------
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
    [Table("ANN1")]
    public partial class ANN1 : Entity
    {
        public ANN1()
        {
          this.ObjectCode= string.Empty;
          this.SeriesName= string.Empty;
          this.BeginStr= string.Empty;
          this.EndStr= string.Empty;
          this.Remark= string.Empty;
          this.Locked= string.Empty;
          this.YearTransf= string.Empty;
          this.Indicator= string.Empty;
          this.Template= string.Empty;
          this.FolioPref= string.Empty;
          this.DocSubType= string.Empty;
          this.IsDigSerie= string.Empty;
          this.SeriesType= string.Empty;
          this.IsManual= string.Empty;
          this.IsForCncl= string.Empty;
          this.AtDocType= string.Empty;
          this.IsElAuth= string.Empty;
          this.CoAccount= string.Empty;
          this.GenPassprt= string.Empty;
          this.CreateDate= DateTime.Now;
          this.UpdateDate= DateTime.Now;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ObjectCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SeriesName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? InitialNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? NextNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? LastNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BeginStr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EndStr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Remark { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? GroupCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Locked { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string YearTransf { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Indicator { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Template { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? NumSize { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FolioPref { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? NextFolio { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DocSubType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? DefESeries { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IsDigSerie { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SeriesType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IsManual { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? BPLId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IsForCncl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AtDocType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IsElAuth { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CoAccount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string GenPassprt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CreateDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? UpdateDate { get; set; }
    }
}