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
    [Table("finance_billapplication_master")]
    public partial class finance_billapplication_master 
    {
        public finance_billapplication_master()
        {
          this.ToCompany= string.Empty;
          this.billType= string.Empty;
          this.isChange= string.Empty;
          this.ApplicationTime= DateTime.Now;
          this.CardName= string.Empty;
          this.TaxSignNo= string.Empty;
          this.Address= string.Empty;
          this.Tel= string.Empty;
          this.BankName= string.Empty;
          this.BankAccount= string.Empty;
          this.CollectStatus= string.Empty;
          this.AccountName= string.Empty;
          this.predictDate= DateTime.Now;
          this.remark1= string.Empty;
          this.remark2= string.Empty;
          this.updatetime= DateTime.Now;
          this.VATbillnum= string.Empty;
          this.BillTime= DateTime.Now;
          this.cancel_remarks= string.Empty;
          this.IsConsuming= string.Empty;
          this.remark= string.Empty;
          this.BillToName= string.Empty;
          this.BillToTel= string.Empty;
          this.BillToAddress= string.Empty;
          this.SpeedNo= string.Empty;
        }


        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? billID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? sbo_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? DocEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ToCompany { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string billType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string isChange { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? Applicantor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? ApplicationTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CardName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string TaxSignNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Address { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Tel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BankName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BankAccount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CollectStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AccountName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? totalmn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Collectfunds { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Uncollectedfunds { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? predictDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? billStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string remark1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string remark2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime updatetime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string VATbillnum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? Billor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? BillTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? job_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string cancel_remarks { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IsConsuming { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string remark { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BillToName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BillToTel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BillToAddress { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SpeedNo { get; set; }
    }
}