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
    [Table("OBNK")]
    public partial class OBNK : Entity
    {
        public OBNK()
        {
          this.AcctName= string.Empty;
          this.Ref= string.Empty;
          this.DueDate= DateTime.Now;
          this.Memo= string.Empty;
          this.DebAmntCur= string.Empty;
          this.CredAmntCu= string.Empty;
          this.DataSource= string.Empty;
          this.ExternCode= string.Empty;
          this.CardCode= string.Empty;
          this.CardName= string.Empty;
          this.DocNum= string.Empty;
          this.PaymCreat= string.Empty;
          this.LineStatus= string.Empty;
          this.DocNumType= string.Empty;
          this.Memo2= string.Empty;
          this.PaymentRef= string.Empty;
          this.autoCreate= string.Empty;
          this.BSLineDate= DateTime.Now;
          this.BSValuDate= DateTime.Now;
          this.Cleared= string.Empty;
          this.OposAct= string.Empty;
          this.BPIBAN= string.Empty;
          this.PmnPstDate= DateTime.Now;
          this.PmnValDate= DateTime.Now;
          this.LnDocDate= DateTime.Now;
          this.PstMethod= string.Empty;
          this.FeeAct= string.Empty;
          this.FeeProfitC= string.Empty;
          this.FeeProj= string.Empty;
          this.BpBankCode= string.Empty;
          this.FeeProfit2= string.Empty;
          this.FeeProfit3= string.Empty;
          this.FeeProfit4= string.Empty;
          this.FeeProfit5= string.Empty;
          this.InfoLog= string.Empty;
          this.LineOrigin= string.Empty;
          this.BPSwift= string.Empty;
          this.Source= string.Empty;
          this.TaxIDNum= string.Empty;
          this.FormatName= string.Empty;
          this.FileCRC= string.Empty;
          this.FolioPref= string.Empty;
          this.BPAcctName= string.Empty;
          this.EnOposAct= string.Empty;
          this.EnBPIBAN= string.Empty;
          this.EncryptIV= string.Empty;
          this.DPPStatus= string.Empty;
          this.CreateDate= DateTime.Now;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? IdNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AcctName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Ref { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? DueDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Memo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? DebAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DebAmntCur { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? CredAmnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CredAmntCu { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? balance { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? BankMatch { get; set; }
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
        public string ExternCode { get; set; }
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
        public int? StatemNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DocNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PaymCreat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string LineStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? VisOrder { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DocNumType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Memo2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PaymentRef { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string autoCreate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? BSLineDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? BSValuDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? InOpCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Cleared { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string OposAct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? DebAmntLC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? CredAmntLC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? ExchngRate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BPIBAN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Fee { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? PmnPstDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? PmnValDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? LnDocDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? VatAmntLC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? VatAmntFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? JDTID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? PmntID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? ObjCrtType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PstMethod { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FeeAct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FeeProfitC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FeeProj { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BpBankCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FeeProfit2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FeeProfit3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FeeProfit4 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FeeProfit5 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? ApplWTSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string InfoLog { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string LineOrigin { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BPSwift { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Source { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? PayOrderNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string TaxIDNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? PONumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FormatName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FileCRC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FolioPref { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? FolioNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? BPLIdPmn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BPAcctName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnOposAct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnBPIBAN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EncryptIV { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DPPStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CreateDate { get; set; }
    }
}