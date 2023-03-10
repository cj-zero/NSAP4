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
    [Table("AINF")]
    public partial class AINF : Entity
    {
        public AINF()
        {
          this.CompnyName= string.Empty;
          this.Flags= string.Empty;
          this.InfoA1= string.Empty;
          this.InfoA2= string.Empty;
          this.LawsSet= string.Empty;
          this.EnblVatGrp= string.Empty;
          this.EnblVTGPmn= string.Empty;
          this.VatOOS_O= string.Empty;
          this.VatOOS_I= string.Empty;
          this.VatStnd_O= string.Empty;
          this.VatStnd_I= string.Empty;
          this.VatExmpt_O= string.Empty;
          this.VatExmpt_I= string.Empty;
          this.VatHalf_O= string.Empty;
          this.VatHalf_I= string.Empty;
          this.VatZrRt_O= string.Empty;
          this.VatZrRt_I= string.Empty;
          this.EnblDctSrc= string.Empty;
          this.EnblRCN= string.Empty;
          this.EnblCorINV= string.Empty;
          this.EnblCshRep= string.Empty;
          this.EnblIRTRep= string.Empty;
          this.EnblTrnRep= string.Empty;
          this.EnblVPMRep= string.Empty;
          this.EnblACTRep= string.Empty;
          this.EnblFTZ= string.Empty;
          this.EnblSlfPCH= string.Empty;
          this.EnblPCHUpd= string.Empty;
          this.EnblRsvINV= string.Empty;
          this.EnblRTDnld= string.Empty;
          this.RTTDnldAdr= string.Empty;
          this.ZPrcCode= string.Empty;
          this.EnblExpns= string.Empty;
          this.EnblRtrRep= string.Empty;
          this.IsEC= string.Empty;
          this.EnblCshDsc= string.Empty;
          this.EnblLostCD= string.Empty;
          this.EnblRealRD= string.Empty;
          this.EnblVTGNoD= string.Empty;
          this.EnblGLManP= string.Empty;
          this.EnblOptVIP= string.Empty;
          this.EnblActCrC= string.Empty;
          this.EnblFrnAct= string.Empty;
          this.EnblECAct= string.Empty;
          this.EnblECRpt= string.Empty;
          this.EnblRound= string.Empty;
          this.EnblPrVatS= string.Empty;
          this.EnblYrTrns= string.Empty;
          this.EnblActTtl= string.Empty;
          this.EnblDocWrn= string.Empty;
          this.EnbSgmnAct= string.Empty;
          this.Rprt1099= string.Empty;
          this.MultiAddss= string.Empty;
          this.EnbDunning= string.Empty;
          this.EnbQtyEDln= string.Empty;
          this.Itw1Date= DateTime.Now;
          this.EnbPayRef= string.Empty;
          this.EnblDfrTax= string.Empty;
          this.EnblClsPr= string.Empty;
          this.EnblTaxEL= string.Empty;
          this.EnableBOE= string.Empty;
          this.EnableWHT= string.Empty;
          this.EnblEquVat= string.Empty;
          this.EnblFAsset= string.Empty;
          this.EnblDoubt= string.Empty;
          this.RateBase= string.Empty;
          this.BOEStatClo= string.Empty;
          this.ARDocsInWT= string.Empty;
          this.BISBnkCnt= string.Empty;
          this.BISRBnkCd= string.Empty;
          this.BISRBnkAc= string.Empty;
          this.BISRBranch= string.Empty;
          this.EnblBPConn= string.Empty;
          this.EnblVATDat= string.Empty;
          this.EnblStAgRp= string.Empty;
          this.EnblCARepo= string.Empty;
          this.EnblMatRev= string.Empty;
          this.EnblMBPRec= string.Empty;
          this.CshDscGros= string.Empty;
          this.EnblTaxInv= string.Empty;
          this.EnblCorAct= string.Empty;
          this.EnblRuDIP= string.Empty;
          this.EnblCurDec= string.Empty;
          this.EnblPayMtd= string.Empty;
          this.EnblBaseUn= string.Empty;
          this.EnblVATAna= string.Empty;
          this.EnblExREnh= string.Empty;
          this.VATGrpCal= string.Empty;
          this.EnblInfla= string.Empty;
          this.EnblLAWHT= string.Empty;
          this.EnblRTWHT= string.Empty;
          this.ChkQunty= string.Empty;
          this.SriMngSys= string.Empty;
          this.BtchMngSys= string.Empty;
          this.SriCreatIn= string.Empty;
          this.EnblFolio= string.Empty;
          this.EnblDocSbT= string.Empty;
          this.IepsPayer= string.Empty;
          this.EnblLATaxS= string.Empty;
          this.EnblDpmJdt= string.Empty;
          this.EnblDownP= string.Empty;
          this.EnblNDdctC= string.Empty;
          this.DocNmMtd= string.Empty;
          this.DoFilter= string.Empty;
          this.EnblOnPDCh= string.Empty;
          this.EnblOnWnCr= string.Empty;
          this.EnblDefInx= string.Empty;
          this.EnblMxComm= string.Empty;
          this.EnblIndxOp= string.Empty;
          this.EnblSbtCVo= string.Empty;
          this.CredSumm= string.Empty;
          this.PostdChk= string.Empty;
          this.PostdCred= string.Empty;
          this.CredVend= string.Empty;
          this.WkoStatus= string.Empty;
          this.DispTrByDf= string.Empty;
          this.stampTax= string.Empty;
          this.BlockZeroQ= string.Empty;
          this.AutoCrIns= string.Empty;
          this.EnbRepomo= string.Empty;
          this.RFCValidat= string.Empty;
          this.RelStkNoPr= string.Empty;
          this.CashDisc= string.Empty;
          this.EnableSMS= string.Empty;
          this.EnblIndic= string.Empty;
          this.EnbFedTax= string.Empty;
          this.EnblCounty= string.Empty;
          this.ChkIntgUpd= string.Empty;
          this.ChkIntgCre= string.Empty;
          this.EnbZeroDec= string.Empty;
          this.EnbDecWord= string.Empty;
          this.ChkWrdOnly= string.Empty;
          this.EnBnkStmnt= string.Empty;
          this.CalcVatGrp= string.Empty;
          this.TaxSysType= string.Empty;
          this.ESEnabled= string.Empty;
          this.RateTotal= string.Empty;
          this.CompanyHis= string.Empty;
          this.EnblAssVal= string.Empty;
          this.CompanySta= string.Empty;
          this.TaxGrpType= string.Empty;
          this.InstallNo= string.Empty;
          this.IsOldPA= string.Empty;
          this.EnbNegDoc= string.Empty;
          this.ArcComp= string.Empty;
          this.UpdatedTF= string.Empty;
          this.DARDBGUID= string.Empty;
          this.NegStoLv= string.Empty;
          this.SPNEnabled= string.Empty;
          this.PrsWkCntEb= string.Empty;
          this.DashbdEb= string.Empty;
          this.BoxEffDate= DateTime.Now;
          this.UpdateDate= DateTime.Now;
          this.CreatedBy= string.Empty;
          this.B1SgtEb= string.Empty;
          this.IsHConEnv= string.Empty;
          this.B1BuzzEb= string.Empty;
          this.IMCEEnable= string.Empty;
          this.DKeyId= string.Empty;
          this.ResetDData= string.Empty;
          this.ConvDifAct= string.Empty;
          this.BasEffDate= DateTime.Now;
          this.oldFxAss= string.Empty;
          this.ColSel= string.Empty;
          this.EnblSPEDUF= string.Empty;
          this.DpmAffTot= string.Empty;
          this.AliasUpd= string.Empty;
          this.TestComp= string.Empty;
          this.SideEnable= string.Empty;
          this.IsPALInit= string.Empty;
          this.ExpEnable= string.Empty;
          this.LastSsrDat= DateTime.Now;
          this.LastSsrHsh= string.Empty;
          this.CpRfshEnbl= string.Empty;
          this.EnbMBFilt= string.Empty;
          this.CompnyGUID= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CompnyName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Flags { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? InfoL1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? InfoL2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string InfoA1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string InfoA2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? ACTStamp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? ADMStamp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? RTTStamp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? CINFStamp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string LawsSet { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblVatGrp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblVTGPmn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string VatOOS_O { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string VatOOS_I { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string VatStnd_O { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string VatStnd_I { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string VatExmpt_O { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string VatExmpt_I { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string VatHalf_O { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string VatHalf_I { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string VatZrRt_O { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string VatZrRt_I { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? MaxActGrps { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblDctSrc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblRCN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblCorINV { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblCshRep { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblIRTRep { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblTrnRep { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblVPMRep { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblACTRep { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblFTZ { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblSlfPCH { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblPCHUpd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblRsvINV { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblRTDnld { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RTTDnldAdr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ZPrcCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblExpns { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblRtrRep { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IsEC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblCshDsc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblLostCD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? NumActLvls { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblRealRD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblVTGNoD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblGLManP { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblOptVIP { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblActCrC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblFrnAct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblECAct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblECRpt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblRound { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblPrVatS { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblYrTrns { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblActTtl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblDocWrn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? ActSegNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnbSgmnAct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? SizeOfSeg0 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? SizeOfSeg1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? SizeOfSeg2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? SizeOfSeg3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? SizeOfSeg4 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? SizeOfSeg5 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? SizeOfSeg6 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? SizeOfSeg7 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? SizeOfSeg8 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? SizeOfSeg9 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Rprt1099 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string MultiAddss { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnbDunning { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnbQtyEDln { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? Itw1Date { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? Itw1Time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? Itw1Count { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnbPayRef { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblDfrTax { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblClsPr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblTaxEL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnableBOE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnableWHT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblEquVat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblFAsset { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblDoubt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RateBase { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BOEStatClo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ARDocsInWT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BISBnkCnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BISRBnkCd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BISRBnkAc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BISRBranch { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblBPConn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblVATDat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblStAgRp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblCARepo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblMatRev { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblMBPRec { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CshDscGros { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblTaxInv { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblCorAct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblRuDIP { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblCurDec { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblPayMtd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblBaseUn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblVATAna { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblExREnh { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string VATGrpCal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? MaxChoose { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblInfla { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblLAWHT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblRTWHT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ChkQunty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SriMngSys { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BtchMngSys { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SriCreatIn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblFolio { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblDocSbT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IepsPayer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? DaysOrdCnc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblLATaxS { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? PercOfAcq { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? MinBaseDoc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblDpmJdt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblDownP { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblNDdctC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DocNmMtd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DoFilter { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblOnPDCh { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblOnWnCr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblDefInx { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblMxComm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblIndxOp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblSbtCVo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? MinAmntOAP { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CredSumm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PostdChk { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PostdCred { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CredVend { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string WkoStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DispTrByDf { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string stampTax { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? MinAmntAL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BlockZeroQ { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AutoCrIns { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnbRepomo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RFCValidat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? MxDcsInPmt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RelStkNoPr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CashDisc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnableSMS { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblIndic { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnbFedTax { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblCounty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? Language { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ChkIntgUpd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ChkIntgCre { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? BisBnkAcKy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnbZeroDec { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnbDecWord { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ChkWrdOnly { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnBnkStmnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CalcVatGrp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string TaxSysType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ESEnabled { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RateTotal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CompanyHis { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblAssVal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CompanySta { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? eFRTActLvs { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string TaxGrpType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string InstallNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IsOldPA { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnbNegDoc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? Algo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ArcComp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string UpdatedTF { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DARDBGUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string NegStoLv { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SPNEnabled { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PrsWkCntEb { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DashbdEb { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? BoxEffDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? UpdateDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? SnapShotId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CreatedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string B1SgtEb { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IsHConEnv { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string B1BuzzEb { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IMCEEnable { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string DKeyId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ResetDData { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ConvDifAct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? BasEffDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string oldFxAss { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? MaxRowsCFL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ColSel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnblSPEDUF { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DpmAffTot { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AliasUpd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? TrailDays { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string TestComp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.Byte[] DashConf { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SideEnable { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IsPALInit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? PANAVer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ExpEnable { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? LastSsrDat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string LastSsrHsh { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? B1iTimeOut { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CpRfshEnbl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? CpRfshIntv { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EnbMBFilt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CompnyGUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? ShutTime { get; set; }
    }
}