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

namespace OpenAuth.Repository.Domain
{
    /// <summary>
	/// 
	/// </summary>
    [Table("crm_ocrd")]
    public partial class crm_ocrd 
    {
        public crm_ocrd()
        {
          this.CardName= string.Empty;
          this.CardFName= string.Empty;
          this.CardType= string.Empty;
          this.CmpPrivate= string.Empty;
          this.Phone1= string.Empty;
          this.Phone2= string.Empty;
          this.Cellular= string.Empty;
          this.Fax= string.Empty;
          this.E_Mail= string.Empty;
          this.CntctPrsn= string.Empty;
          this.Notes= string.Empty;
          this.LicTradNum= string.Empty;
          this.Free_Text= string.Empty;
          this.Currency= string.Empty;
          this.BillToDef= string.Empty;
          this.ShipToDef= string.Empty;
          this.ZipCode= string.Empty;
          this.MailZipCod= string.Empty;
          this.Building= string.Empty;
          this.MailBuildi= string.Empty;
          this.County= string.Empty;
          this.MailCounty= string.Empty;
          this.City= string.Empty;
          this.MailCity= string.Empty;
          this.AddrType= string.Empty;
          this.MailAddrTy= string.Empty;
          this.Address= string.Empty;
          this.MailAddres= string.Empty;
          this.State1= string.Empty;
          this.State2= string.Empty;
          this.Country= string.Empty;
          this.MailCountr= string.Empty;
          this.DflAccount= string.Empty;
          this.DflBranch= string.Empty;
          this.BankCode= string.Empty;
          this.AddID= string.Empty;
          this.FatherType= string.Empty;
          this.DscntRel= string.Empty;
          this.DataSource= string.Empty;
          this.CrCardNum= string.Empty;
          this.CardValid= DateTime.Now;
          this.LocMth= string.Empty;
          this.validFor= string.Empty;
          this.validFrom= DateTime.Now;
          this.validTo= DateTime.Now;
          this.frozenFor= string.Empty;
          this.frozenFrom= DateTime.Now;
          this.frozenTo= DateTime.Now;
          this.ValidComm= string.Empty;
          this.FrozenComm= string.Empty;
          this.VatGroup= string.Empty;
          this.ObjType= string.Empty;
          this.Indicator= string.Empty;
          this.DebPayAcct= string.Empty;
          this.DocEntry= 0;
          this.HouseBank= string.Empty;
          this.HousBnkCry= string.Empty;
          this.HousBnkAct= string.Empty;
          this.HousBnkBrn= string.Empty;
          this.ProjectCod= string.Empty;
          this.VatIdUnCmp= string.Empty;
          this.AgentCode= string.Empty;
          this.SelfInvoic= string.Empty;
          this.DeferrTax= string.Empty;
          this.LetterNum= string.Empty;
          this.FromDate= DateTime.Now;
          this.ToDate= DateTime.Now;
          this.WTLiable= string.Empty;
          this.CrtfcateNO= string.Empty;
          this.ExpireDate= DateTime.Now;
          this.NINum= string.Empty;
          this.Industry= string.Empty;
          this.Business= string.Empty;
          this.AliasName= string.Empty;
          this.GTSRegNum= string.Empty;
          this.GTSBankAct= string.Empty;
          this.GTSBilAddr= string.Empty;
          this.HsBnkSwift= string.Empty;
          this.HsBnkIBAN= string.Empty;
          this.DflSwift= string.Empty;
          this.QryGroup1= string.Empty;
          this.QryGroup2= string.Empty;
          this.QryGroup3= string.Empty;
          this.QryGroup4= string.Empty;
          this.QryGroup5= string.Empty;
          this.QryGroup6= string.Empty;
          this.QryGroup7= string.Empty;
          this.QryGroup8= string.Empty;
          this.QryGroup9= string.Empty;
          this.QryGroup10= string.Empty;
          this.QryGroup11= string.Empty;
          this.QryGroup12= string.Empty;
          this.QryGroup13= string.Empty;
          this.QryGroup14= string.Empty;
          this.QryGroup15= string.Empty;
          this.QryGroup16= string.Empty;
          this.QryGroup17= string.Empty;
          this.QryGroup18= string.Empty;
          this.QryGroup19= string.Empty;
          this.QryGroup20= string.Empty;
          this.QryGroup21= string.Empty;
          this.QryGroup22= string.Empty;
          this.QryGroup23= string.Empty;
          this.QryGroup24= string.Empty;
          this.QryGroup25= string.Empty;
          this.QryGroup26= string.Empty;
          this.QryGroup27= string.Empty;
          this.QryGroup28= string.Empty;
          this.QryGroup29= string.Empty;
          this.QryGroup30= string.Empty;
          this.QryGroup31= string.Empty;
          this.QryGroup32= string.Empty;
          this.QryGroup33= string.Empty;
          this.QryGroup34= string.Empty;
          this.QryGroup35= string.Empty;
          this.QryGroup36= string.Empty;
          this.QryGroup37= string.Empty;
          this.QryGroup38= string.Empty;
          this.QryGroup39= string.Empty;
          this.QryGroup40= string.Empty;
          this.QryGroup41= string.Empty;
          this.QryGroup42= string.Empty;
          this.QryGroup43= string.Empty;
          this.QryGroup44= string.Empty;
          this.QryGroup45= string.Empty;
          this.QryGroup46= string.Empty;
          this.QryGroup47= string.Empty;
          this.QryGroup48= string.Empty;
          this.QryGroup49= string.Empty;
          this.QryGroup50= string.Empty;
          this.QryGroup51= string.Empty;
          this.QryGroup52= string.Empty;
          this.QryGroup53= string.Empty;
          this.QryGroup54= string.Empty;
          this.QryGroup55= string.Empty;
          this.QryGroup56= string.Empty;
          this.QryGroup57= string.Empty;
          this.QryGroup58= string.Empty;
          this.QryGroup59= string.Empty;
          this.QryGroup60= string.Empty;
          this.QryGroup61= string.Empty;
          this.QryGroup62= string.Empty;
          this.QryGroup63= string.Empty;
          this.QryGroup64= string.Empty;
          this.U_PYSX= string.Empty;
          this.U_Name= string.Empty;
          this.U_FName= string.Empty;
          this.U_FPLB= string.Empty;
          this.U_Prefix= string.Empty;
          this.U_Suffix= string.Empty;
          this.CreateDate= DateTime.Now;
          this.upd_dt = DateTime.Now;
          this.IntrntSite= string.Empty;
          this.U_is_reseller= string.Empty;
          this.U_EndCustomerName= string.Empty;
          this.U_EndCustomerContact= string.Empty;
          this.CardCode = string.Empty;
        }

        public int? sbo_id { get; set; }
        public DateTime upd_dt { get; set; }
        public string CardCode { get; set; }
        public int? SlpCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CardName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CardFName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CardType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? GroupCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CmpPrivate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Phone1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Phone2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Cellular { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Fax { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string E_Mail { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CntctPrsn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Notes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Balance { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? DNotesBal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? OrdersBal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? OprCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? GroupNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string LicTradNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? ListNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? DNoteBalSy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? OrderBalSy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Free_Text { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        /// <summary>
        /// 
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BillToDef { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ShipToDef { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ZipCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string MailZipCod { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Building { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string MailBuildi { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string County { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string MailCounty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string City { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string MailCity { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AddrType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string MailAddrTy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Address { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string MailAddres { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string State1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string State2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Country { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string MailCountr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DflAccount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DflBranch { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BankCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AddID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FatherType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? DscntObjct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DscntRel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DataSource { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? Priority { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? CreditCard { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CrCardNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CardValid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string LocMth { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string validFor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? validFrom { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? validTo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string frozenFor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? frozenFrom { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? frozenTo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ValidComm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FrozenComm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string VatGroup { get; set; }
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
        public string Indicator { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? ShipType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DebPayAcct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int DocEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string HouseBank { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string HousBnkCry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string HousBnkAct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string HousBnkBrn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ProjectCod { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string VatIdUnCmp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AgentCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? TolrncDays { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SelfInvoic { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DeferrTax { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string LetterNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? MaxAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? FromDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? ToDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string WTLiable { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CrtfcateNO { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? IndustryC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? ExpireDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string NINum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Industry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Business { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AliasName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? DfTcnician { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? Territory { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string GTSRegNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string GTSBankAct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string GTSBilAddr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string HsBnkSwift { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string HsBnkIBAN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DflSwift { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Sales_Volume { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Service_Fees { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public sbyte? wait_assign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup4 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup5 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup6 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup7 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup8 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup9 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup10 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup11 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup12 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup13 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup14 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup15 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup16 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup17 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup18 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup19 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup20 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup21 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup22 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup23 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup24 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup25 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup26 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup27 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup28 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup29 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup30 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup31 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup32 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup33 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup34 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup35 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup36 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup37 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup38 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup39 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup40 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup41 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup42 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup43 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup44 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup45 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup46 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup47 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup48 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup49 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup50 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup51 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup52 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup53 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup54 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup55 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup56 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup57 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup58 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup59 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup60 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup61 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup62 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup63 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QryGroup64 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Sales_Volume_in_one { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Sales_Volume_out_one { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? sum_qty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? sum_qty_in_one { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_PYSX { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_FName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_FPLB { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_Prefix { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_Suffix { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CreateDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        /// <summary>
        /// 
        /// </summary>
        public string IntrntSite { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_is_reseller { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_EndCustomerName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_EndCustomerContact { get; set; }

        /// <summary>
        /// 所属行业(还有一些字段是数据库有的,实体没有)
        /// </summary>
        public string U_CompSector { get; set; }

        /// <summary>
        /// 贸易类型
        /// </summary>
        public string U_TradeType { get; set; }

        /// <summary>
        /// 客户来源
        /// </summary>
        public string U_ClientSource { get; set; }

        /// <summary>
        /// 客户类型
        /// </summary>
        public string U_CardTypeStr { get; set; }

        /// <summary>
        /// 人员规模
        /// </summary>
        public string U_StaffScale { get; set; }
    }
}