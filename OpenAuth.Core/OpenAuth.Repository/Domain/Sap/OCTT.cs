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
    [Table("OCTT")]
    public partial class OCTT : Entity
    {
        public OCTT()
        {
          this.Deleted= string.Empty;
          this.Renewal= string.Empty;
          this.RemindUnit= string.Empty;
          this.ResponsUnt= string.Empty;
          this.Descriptio= string.Empty;
          this.CntrctType= string.Empty;
          this.MonEnabled= string.Empty;
          this.TueEnabled= string.Empty;
          this.WedEnabled= string.Empty;
          this.ThuEnabled= string.Empty;
          this.FriEnabled= string.Empty;
          this.SatEnabled= string.Empty;
          this.SunEnabled= string.Empty;
          this.InclParts= string.Empty;
          this.InclWork= string.Empty;
          this.InclTravel= string.Empty;
          this.Attachment= string.Empty;
          this.CreateDate= DateTime.Now;
          this.Remark= string.Empty;
          this.InclHldays= string.Empty;
          this.ResponseU= string.Empty;
          this.U_PRX_SID= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Deleted { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Renewal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? RemindVal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RemindUnit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? Duration { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? ResponsVal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ResponsUnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Descriptio { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? PriceList { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CntrctType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string MonEnabled { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string TueEnabled { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string WedEnabled { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ThuEnabled { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FriEnabled { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SatEnabled { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SunEnabled { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? MonStart { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? MonEnd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? TueStart { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? TueEnd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? WedStart { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? WedEnd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? ThuStart { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? ThuEnd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? FriStart { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? FriEnd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? SatStart { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? SatEnd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? SunStrart { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? SunEnd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string InclParts { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string InclWork { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string InclTravel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Attachment { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CreateDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Remark { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string InclHldays { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? ResponseV { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ResponseU { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? AtcEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_PRX_SID { get; set; }
    }
}