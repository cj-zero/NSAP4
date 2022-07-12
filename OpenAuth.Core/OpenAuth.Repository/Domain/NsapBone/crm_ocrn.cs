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

namespace OpenAuth.Repository.Domain
{
    /// <summary>
	/// 
	/// </summary>
    [Table("crm_ocrn")]
    public partial class crm_ocrn 
    {
        public crm_ocrn()
        {
          this.CurrName= string.Empty;
          this.ChkName= string.Empty;
          this.Chk100Name= string.Empty;
          this.DocCurrCod= string.Empty;
          this.FrgnName= string.Empty;
          this.F100Name= string.Empty;
          this.Locked= string.Empty;
          this.DataSource= string.Empty;
          this.ISRCalc= string.Empty;
          this.RoundPym= string.Empty;
          this.ConvUnit= string.Empty;
          this.BaseCurr= string.Empty;
          this.ChkNamePl= string.Empty;
          this.Chk100NPl= string.Empty;
          this.FrgnNamePl= string.Empty;
          this.F100NamePl= string.Empty;
          this.ISOCurrCod= string.Empty;
          this.ISOCurrNum= string.Empty;
        }

        public string CurrCode { get; set; }
        public int sbo_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CurrName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ChkName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Chk100Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DocCurrCod { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FrgnName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string F100Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Locked { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DataSource { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? RoundSys { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? Decimals { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ISRCalc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RoundPym { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ConvUnit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BaseCurr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Factor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ChkNamePl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Chk100NPl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FrgnNamePl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string F100NamePl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ISOCurrCod { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? MaxInDiff { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? MaxOutDiff { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? MaxInPcnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? MaxOutPcnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ISOCurrNum { get; set; }
    }
}