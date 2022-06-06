using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.App.WMS.Request
{
    public class ProductReceiptDetailReq
    {
        public int sbo_id { get; set; }
        public int DocEntry { get; set; }
        public int LineNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? TargetType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? TrgetEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string BaseRef { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? BaseType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? BaseEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? BaseLine { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string LineStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string ItemCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Dscription { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? Quantity { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public System.DateTime? ShipDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? OpenQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? Price { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Currency { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? Rate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? DiscPrcnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? LineTotal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? TotalFrgn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? OpenSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? OpenSumFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string VendorNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string SerialNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string WhsCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? SlpCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? Commission { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string TreeType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string AcctCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string TaxStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? GrossBuyPr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? PriceBefDi { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public System.DateTime? DocDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? Flags { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? OpenCreQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string UseBaseUn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string SubCatNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string BaseCard { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string InvntSttus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string OcrCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Project { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string CodeBars { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? VatPrcnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string VatGroup { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? PriceAfVAT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? Height1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? Hght1Unit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? Height2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? Hght2Unit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? Width1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? Wdth1Unit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? Width2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? Wdth2Unit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? Length1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? Len1Unit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? length2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? Len2Unit { get; set; }
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
        
        public decimal? Weight1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? Wght1Unit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? Weight2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? Wght2Unit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? Factor1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? Factor2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? Factor3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? Factor4 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? PackQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string UpdInvntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? BaseDocNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string BaseAtCard { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string SWW { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? VatSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? VatSumFrgn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? FinncPriod { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string ObjType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string BlockNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? DedVatSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? DedVatSumF { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string IsAqcuistn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? DistribSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? DstrbSumFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? GrssProfit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? GrssProfFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? VisOrder { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? INMPrice { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? PoTrgNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string PoTrgEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string DropShip { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? PoLineNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Address { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string TaxCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string TaxType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string OrigItem { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string BackOrdr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string FreeTxt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string PickStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? PickOty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? PickIdNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? TrnsCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? VatAppld { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? VatAppldFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? BaseQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? BaseOpnQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? VatDscntPr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string WtLiable { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string DeferrTax { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? EquVatPer { get; set; }
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
        
        public decimal? LineVat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? LineVatlF { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string unitMsr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? NumPerMsr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string CEECFlag { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? ToStock { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? ToDiff { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? ExciseAmt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? TaxPerUnit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? TotInclTax { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string CountryOrg { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? StckDstSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? ReleasQtty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string LineType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string TranType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Text { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? StockPrice { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string ConsumeFCT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? LstByDsSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? StckINMPr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? LstBINMPr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? StckDstFc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? LstByDsFc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? StockSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? StockSumFc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? StckSumApp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? StckAppFc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string ShipToCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string ShipToDesc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? StckAppD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? StckAppDFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string BasePrice { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? GTotal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? GTotalFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string DistribExp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string DescOW { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string DetailsOW { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? GrossBase { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? VatWoDpm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? VatWoDpmFc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string CFOPCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string CSTCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? Usage { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string TaxOnly { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string WtCalced { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? QtyToShip { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? DelivrdQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? OrderedQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string CogsOcrCod { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? CiOppLineN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string CogsAcct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string ChgAsmBoMW { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public System.DateTime? ActDelDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? TaxDistSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? TaxDistSFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string PostTax { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Excisable { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? AssblValue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string LnExcised { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? LocCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? StockValue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? GPTtlBasPr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string unitMsr2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? NumPerMsr2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string SpecPrice { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string ExLineNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string isSrvCall { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? PQTReqQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public System.DateTime? PQTReqDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? PcDocType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? PcQuantity { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string LinManClsd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string VatGrpSrc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string NoInvtryMv { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? ActBaseEnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? ActBaseLn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? ActBaseNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? OpenRtnQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? AgrNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? AgrLnNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string CredOrigin { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? Surpluses { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? DefBreak { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? Shortages { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_WLLY { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_YYFX { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_XWJBH { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_ZWJBH { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_IG_DW { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? U_ZDJG { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_ZXDH { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_TYWP { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_CPH { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_TYSL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_PRX_SID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_PRX_VMas { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_PRX_VNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_PRX_Note { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_A_CCNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_A_EDATE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_A_SECCOD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_A_ADDR1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_A_ADDR2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_A_ADDR3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_A_CITY { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_A_ZIP { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_A_CTYPE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_A_CREFNO { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_A_ADATE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_A_ACODE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_A_STATE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_A_CAMT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_A_PAYT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_BNIncTrm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_BNTrnMod { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? U_CCSJ { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_PDXX { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_BXSJ { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public short? U_LineID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_ZS { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? U_PSD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? U_TCBL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? U_TCJE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? U_KJFY { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? U_YFTCBL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? U_YFTCJE { get; set; }

        public string CustomFields { get; set; }
    }
}
