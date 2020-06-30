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
    [Table("ODPT")]
    public partial class ODPT : Entity
    {
        public ODPT()
        {
          this.DeposType= string.Empty;
          this.DeposDate= DateTime.Now;
          this.DeposCurr= string.Empty;
          this.BanckAcct= string.Empty;
          this.Memo= string.Empty;
          this.Ref1= string.Empty;
          this.Ref2= string.Empty;
          this.Splited= string.Empty;
          this.VatAct= string.Empty;
          this.ComissAct= string.Empty;
          this.ComissDate= DateTime.Now;
          this.TaxDate= DateTime.Now;
          this.DataSource= string.Empty;
          this.ObjType= string.Empty;
          this.CommisVat= string.Empty;
          this.Project= string.Empty;
          this.OcrCode= string.Empty;
          this.OcrCode2= string.Empty;
          this.OcrCode3= string.Empty;
          this.OcrCode4= string.Empty;
          this.OcrCode5= string.Empty;
          this.ComisCurr= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DeposType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? DeposDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DeposCurr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BanckAcct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Memo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? LocTotal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? FcTotal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? SysTotal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? TransAbs { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Ref1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Ref2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? DocRate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Splited { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string VatAct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ComissAct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? VatTotal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Comission { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? ComissDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? TaxDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DataSource { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ObjType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? FinncPriod { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? VatTotlSys { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? ComissnSys { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CommisVat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Project { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string OcrCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string OcrCode2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string OcrCode3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string OcrCode4 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string OcrCode5 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? ComisFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? VatTotlFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ComisCurr { get; set; }
    }
}