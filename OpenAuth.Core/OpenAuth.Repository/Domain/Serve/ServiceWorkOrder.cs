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
    [Table("serviceworkorder")]
    public partial class ServiceWorkOrder : Entity
    {
        public ServiceWorkOrder()
        {
          this.ServiceOrderId= 0;
          this.Province= string.Empty;
          this.City= string.Empty;
          this.Addr= string.Empty;
          this.SubmitDate= DateTime.Now;
          this.SubmitUserId= string.Empty;
          this.RecepUserId= string.Empty;
          this.Remark= string.Empty;
          this.FromTheme= string.Empty;
          this.ProblemTypeId= string.Empty;
          this.MaterialCode= string.Empty;
          this.MaterialDescription= string.Empty;
          this.ManufacturerSerialNumber= string.Empty;
          this.InternalSerialNumber= string.Empty;
          this.WarrantyEndDate= DateTime.Now;
          this.CreateTime= DateTime.Now;
          this.BookingDate= DateTime.Now;
          this.VisitTime= DateTime.Now;
          this.LiquidationDate= DateTime.Now;
          this.SolutionId= string.Empty;
          this.AddressDesignator= string.Empty;
          this.CompletionReportId= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int ServiceOrderId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? Priority { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? FeeType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Province { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string City { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Addr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Longitude { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Latitude { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? SubmitDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string SubmitUserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? AppUserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string RecepUserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Remark { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? CurrentUserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FromTheme { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? FromId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string ProblemTypeId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? FromType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string MaterialCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string MaterialDescription { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ManufacturerSerialNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string InternalSerialNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? WarrantyEndDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CreateTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? BookingDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? VisitTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? LiquidationDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string SolutionId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AddressDesignator { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string CompletionReportId { get; set; }
    }
}