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
    [Table("OTRA")]
    public partial class OTRA : Entity
    {
        public OTRA()
        {
          this.Printed= string.Empty;
          this.ObjType= string.Empty;
          this.DocDate= DateTime.Now;
          this.DocDueDate= DateTime.Now;
          this.CardCode= string.Empty;
          this.CardName= string.Empty;
          this.FreZoneNum= string.Empty;
          this.Shiper= string.Empty;
          this.Address= string.Empty;
          this.FromCntry= string.Empty;
          this.ToCntry= string.Empty;
          this.FromPort= string.Empty;
          this.ToPort= string.Empty;
          this.AirPlane= string.Empty;
          this.AirPlanNum= string.Empty;
          this.Ref1= string.Empty;
          this.Ref2= string.Empty;
          this.Comments= string.Empty;
          this.PartSupply= string.Empty;
          this.CreateDate= DateTime.Now;
          this.UpdateDate= DateTime.Now;
          this.NumForPrn= string.Empty;
          this.PurPackMsr= string.Empty;
          this.DataSource= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? DocNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? BaseDocNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Printed { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ObjType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? DocDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? DocDueDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CardCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CardName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FreZoneNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Shiper { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Address { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FromCntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ToCntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FromPort { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ToPort { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AirPlane { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AirPlanNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? TotalFob { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Taxes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Insurance { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Others { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? TotalCif { get; set; }
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
        public string Comments { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? GroupNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? DocTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? SlpCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? TrnspCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PartSupply { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? ImportEnt { get; set; }
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
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string NumForPrn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PurPackMsr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? Series { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? BaseDocObj { get; set; }
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
    }
}