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
    [Table("INC1")]
    public partial class INC1 : Entity
    {
        public INC1()
        {
          this.ItemCode= string.Empty;
          this.ItemDesc= string.Empty;
          this.Freeze= string.Empty;
          this.WhsCode= string.Empty;
          this.Counted= string.Empty;
          this.Remark= string.Empty;
          this.BarCode= string.Empty;
          this.InvUoM= string.Empty;
          this.CountDate= DateTime.Now;
          this.TargetRef= string.Empty;
          this.ProjCode= string.Empty;
          this.OcrCode= string.Empty;
          this.LineStatus= string.Empty;
          this.OcrCode2= string.Empty;
          this.OcrCode3= string.Empty;
          this.OcrCode4= string.Empty;
          this.OcrCode5= string.Empty;
          this.SuppCatNum= string.Empty;
          this.PrefVendor= string.Empty;
          this.IUomEntry= string.Empty;
          this.UomCode= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ItemCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ItemDesc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Freeze { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string WhsCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? InWhsQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Counted { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? CountQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? CountQtyT1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? CountQtyT2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Remark { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BarCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string InvUoM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Difference { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? DiffPercen { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CountDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? CountTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string TargetRef { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? TargetType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? TargetEntr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? TargetLine { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ProjCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string OcrCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string LineStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? BinEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? VisOrder { get; set; }
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
        public short? FirmCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SuppCatNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PrefVendor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? LogIns { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? UgpEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IUomEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? CountDiff { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? CountDiffP { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string UomCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? UomQty { get; set; }
    }
}