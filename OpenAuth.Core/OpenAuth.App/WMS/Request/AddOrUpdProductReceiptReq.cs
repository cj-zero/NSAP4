using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.AutoMapper;
using System.ComponentModel.DataAnnotations.Schema;
using OpenAuth.Repository.Domain;

namespace OpenAuth.App.WMS.Request
{
    [Table("product_oign")]
    [AutoMapTo(typeof(product_oign))]
    public class AddOrUpdProductReceiptReq
    {
        public int sbo_id { get; set; }
        public int DocEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int DocNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string DocType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string CANCELED { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Handwrtten { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Printed { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string DocStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string InvntSttus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Transfered { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string ObjType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public System.DateTime? DocDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public System.DateTime? DocDueDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string CardCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string CardName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Address { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string NumAtCard { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? VatPercent { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? VatSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? VatSumFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? DiscPrcnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? DiscSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? DiscSumFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string DocCur { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? DocRate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? DocTotal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? DocTotalFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? PaidToDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? PaidFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? GrosProfit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? GrosProfFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Ref1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Ref2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Comments { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string JrnlMemo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? TransId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? ReceiptNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? GroupNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? DocTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? SlpCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? TrnspCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string PartSupply { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Confirmed { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? GrossBase { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? ImportEnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string CreateTran { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string SummryType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string UpdInvnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string UpdCardBal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? Flags { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string InvntDirec { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? CntctCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string ShowSCN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string FatherCard { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string CurSource { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string FatherType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public System.DateTime? UpdateDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string IsICT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public System.DateTime? CreateDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? Volume { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? VolUnit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? Weight { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? WeightUnit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? Series { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public System.DateTime? TaxDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Filler { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string DataSource { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string StampNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string isCrin { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? FinncPriod { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string selfInv { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? VatPaid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? VatPaidFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string WddStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? draftKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? TotalExpns { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? TotalExpFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? DunnLevel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Address2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Exported { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? StationID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Indicator { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string NetProc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? AqcsTax { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? AqcsTaxFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? CashDiscPr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? CashDiscnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? CashDiscFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string ShipToCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string LicTradNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string PaymentRef { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? WTSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? WTSumSC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? RoundDif { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? RoundDifFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string submitted { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string PoPrss { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Rounding { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string RevisionPo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short Segment { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public System.DateTime? ReqDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public System.DateTime? CancelDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string PickStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Pick { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string BlockDunn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string PeyMethod { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string PayBlock { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? PayBlckRef { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string MaxDscn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Reserve { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? Max1099 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string PickRmrk { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? ExpAppl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? ExpApplFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Project { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string DeferrTax { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string LetterNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public System.DateTime? FromDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public System.DateTime? ToDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? WTApplied { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? WTAppliedF { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string BoeReserev { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string AgentCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? EquVatSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? EquVatSumF { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? Installmnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string VATFirst { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? NnSbAmnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? NbSbAmntFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? ExepAmnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? ExepAmntFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public System.DateTime? VatDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string CorrExt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? CorrInv { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? NCorrInv { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string CEECFlag { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? BaseAmnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? BaseAmntFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string CtlAccount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? BPLId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string BPLName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string VATRegNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string TxInvRptNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public System.DateTime? TxInvRptDt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string KVVATCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string WTDetails { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? SumAbsId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public System.DateTime? SumRptDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string PIndicator { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string ManualNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string UseShpdGd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? BaseVtAt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? BaseVtAtFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? NnSbVAt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? NbSbVAtFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? ExptVAt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? ExptVAtFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? LYPmtAt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? LYPmtAtFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? ExpAnSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? ExpAnFrgn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string DocSubType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string DpmStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? DpmAmnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? DpmAmntFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string DpmDrawn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? DpmPrcnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? PaidSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? PaidSumFc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? DpmAppl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? DpmApplFc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Posted { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string BPChCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? BPChCntc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string PayToCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string IsPaytoBnk { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string BnkCntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string BankCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string BnkAccount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string BnkBranch { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string isIns { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string TrackNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string VersionNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string BPNameOW { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string BillToOW { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string ShipToOW { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string RetInvoice { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public System.DateTime? ClsDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? MInvNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public System.DateTime? MInvDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? SeqCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Model { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? TaxOnExp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? TaxOnExpFc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? TaxOnExAp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? TaxOnExApF { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string LastPmnTyp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string UseCorrVat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string BlkCredMmo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string OpenForLaC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Excised { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public System.DateTime? ExcRefDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string ExcRmvTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? SrvGpPrcnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? DepositNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string CertNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string DutyStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string AutoCrtFlw { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public System.DateTime? FlwRefDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string FlwRefNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? VatJENum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? DpmVat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? DpmVatFc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? DpmAppVat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? DpmAppVatF { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string InsurOp347 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string IgnRelDoc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string BuildDesc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string ResidenNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? Checker { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? Payee { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? CopyNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string SSIExmpt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? PQTGrpSer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? PQTGrpNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string PQTGrpHW { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string ReopOriDoc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string ReopManCls { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string DocManClsd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? ClosingOpt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public System.DateTime? SpecDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Ordered { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string NTSApprov { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? NTSWebSite { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string NTSeTaxNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string NTSApprNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string PayDuMonth { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? ExtraMonth { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? ExtraDays { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? CdcOffset { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string CertifNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string OnlineQuo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string POSEqNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string POSManufSN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? POSCashN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? CUP { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? CIG { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string DpmAsDscnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string SupplCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string GTSRlvnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? BaseDisc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? BaseDiscFc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? BaseDiscPr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? CreateTS { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? UpdateTS { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string SrvTaxRule { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? AnnInvDecR { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Supplier { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? AgrNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string IsAlt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? AltBaseTyp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? AltBaseEnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? PaidDpm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? PaidDpmF { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_CPH { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_YSQX { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_THYY { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_SL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? U_YF { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? U_KDF { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? U_BZF { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? U_YCF { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? U_ZJJE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_YGMD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_OC_CN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_OC_AD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_OC_NA { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_OC_TE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_PRX_SID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_PRX_TkNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_PRX_SRVR { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_ShipName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_S_YWF { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_SMAZ { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_job_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_FPLB { get; set; }

        public virtual IList<ProductReceiptDetailReq> ProductReceiptDetailReqs { get; set; }
    }
}
