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
    [Table("OCHO")]
    public partial class OCHO : Entity
    {
        public OCHO()
        {
          this.BankNum= string.Empty;
          this.Branch= string.Empty;
          this.BankName= string.Empty;
          this.CheckDate= DateTime.Now;
          this.DpstAcct= string.Empty;
          this.AcctNum= string.Empty;
          this.Details= string.Empty;
          this.TransRef= string.Empty;
          this.PmntDate= DateTime.Now;
          this.Trnsfrable= string.Empty;
          this.VendorCode= string.Empty;
          this.Currency= string.Empty;
          this.Canceled= string.Empty;
          this.CardOrAcct= string.Empty;
          this.Printed= string.Empty;
          this.VendorName= string.Empty;
          this.TotalWords= string.Empty;
          this.Signature= string.Empty;
          this.CheckAcct= string.Empty;
          this.Address= string.Empty;
          this.CreateJdt= string.Empty;
          this.UpdateDate= DateTime.Now;
          this.CreateDate= DateTime.Now;
          this.VatCalcult= string.Empty;
          this.TaxDate= DateTime.Now;
          this.DataSource= string.Empty;
          this.Transfered= string.Empty;
          this.ObjType= string.Empty;
          this.CountryCod= string.Empty;
          this.AddrName= string.Empty;
          this.PrnConfrm= string.Empty;
          this.CancelDate= DateTime.Now;
          this.ManualChk= string.Empty;
          this.BPLName= string.Empty;
          this.VATRegNum= string.Empty;
          this.Endorse= string.Empty;
          this.DPPStatus= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? CheckNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BankNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Branch { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BankName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CheckDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DpstAcct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? DpstBranch { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AcctNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Details { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string TransRef { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? PmntDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? PmntNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? CheckSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Trnsfrable { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string VendorCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Currency { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Canceled { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CardOrAcct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Printed { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string VendorName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string TotalWords { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Signature { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CheckAcct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? TransNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? LinesSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Deduction { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? DdctPrcnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Address { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CreateJdt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? UpdateDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CreateDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? VatTotal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string VatCalcult { get; set; }
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
        public decimal? SumRfndCln { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? PrintedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Transfered { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? Instance { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? LogInstanc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ObjType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CountryCod { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AddrName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PrnConfrm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? BnkActKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CancelDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ManualChk { get; set; }
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
        public string BPLName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string VATRegNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Endorse { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? EndorsChNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DPPStatus { get; set; }
    }
}