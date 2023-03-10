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
    [Table("OSCL")]
    public partial class OSCL 
    {
        public OSCL()
        {
          this.subject= string.Empty;
          this.customer= string.Empty;
          this.custmrName= string.Empty;
          this.manufSN= string.Empty;
          this.internalSN= string.Empty;
          this.cntrctDate= DateTime.Now;
          this.resolDate= DateTime.Now;
          this.free_1= string.Empty;
          this.free_2= DateTime.Now;
          this.itemCode= string.Empty;
          this.itemName= string.Empty;
          this.priority= string.Empty;
          this.descrption= string.Empty;
          this.objType= string.Empty;
          this.createDate= DateTime.Now;
          this.closeDate= DateTime.Now;
          this.updateDate= DateTime.Now;
          this.isEntitled= string.Empty;
          this.resolution= string.Empty;
          this.isQueue= string.Empty;
          this.Queue= string.Empty;
          this.resolOnDat= DateTime.Now;
          this.respByDate= DateTime.Now;
          this.respOnDate= DateTime.Now;
          this.AssignDate= DateTime.Now;
          this.Transfered= string.Empty;
          this.DocNum= 0;
          this.Handwrtten= string.Empty;
          this.PIndicator= string.Empty;
          this.StartDate= DateTime.Now;
          this.EndDate= DateTime.Now;
          this.DurType= string.Empty;
          this.Reminder= string.Empty;
          this.RemType= string.Empty;
          this.RemDate= DateTime.Now;
          this.RemSent= string.Empty;
          this.AddrName= string.Empty;
          this.AddrType= string.Empty;
          this.Street= string.Empty;
          this.City= string.Empty;
          this.Room= string.Empty;
          this.State= string.Empty;
          this.Country= string.Empty;
          this.DisplInCal= string.Empty;
          this.SupplCode= string.Empty;
          this.Attachment= string.Empty;
          this.NumAtCard= string.Empty;
          this.BPType= string.Empty;
          this.Telephone= string.Empty;
          this.BPPhone1= string.Empty;
          this.BPPhone2= string.Empty;
          this.BPCellular= string.Empty;
          this.BPFax= string.Empty;
          this.BPShipCode= string.Empty;
          this.BPShipAddr= string.Empty;
          this.BPBillCode= string.Empty;
          this.BPBillAddr= string.Empty;
          this.BPE_Mail= string.Empty;
          this.BPProjCode= string.Empty;
          this.BPContact= string.Empty;
          this.DPPStatus= string.Empty;
          this.EncryptIV= string.Empty;
          this.U_FYMX= string.Empty;
          this.U_SPZT= string.Empty;
          this.U_FWXWBG= string.Empty;
          this.U_FWLX= string.Empty;
          this.U_job_id= string.Empty;
        }

        public int? callID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string subject { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string customer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string custmrName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? contctCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string manufSN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string internalSN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? contractID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? cntrctDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? resolDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? resolTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string free_1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? free_2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? origin { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string itemCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string itemName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? itemGroup { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string priority { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? callType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? problemTyp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? assignee { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string descrption { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string objType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? logInstanc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? userSign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? createDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? createTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? closeDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? closeTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? userSign2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? updateDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? SCL1Count { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? SCL2Count { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string isEntitled { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? insID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? technician { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string resolution { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? Scl1NxtLn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? Scl2NxtLn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? Scl3NxtLn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? Scl4NxtLn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? Scl5NxtLn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string isQueue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Queue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? resolOnDat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? resolOnTim { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? respByDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? respByTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? respOnDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? respOnTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? respAssign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? AssignDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? AssignTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UpdateTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? responder { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Transfered { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short Instance { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int DocNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? Series { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Handwrtten { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PIndicator { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? StartDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? StartTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? EndDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? EndTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Duration { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DurType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Reminder { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? RemQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RemType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? RemDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RemSent { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? RemTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? Location { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AddrName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AddrType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Street { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string City { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Room { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string State { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Country { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DisplInCal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SupplCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Attachment { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? AtcEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string NumAtCard { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? ProSubType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BPType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Telephone { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BPPhone1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BPPhone2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BPCellular { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BPFax { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BPShipCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BPShipAddr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BPBillCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BPBillAddr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? BPTerrit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BPE_Mail { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BPProjCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BPContact { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? OwnerCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DPPStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EncryptIV { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_FYMX { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? U_CCF { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? U_ZSF { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? U_BZ { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? U_CF { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? U_QT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? U_ZJ { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_SPZT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_FWXWBG { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_FWLX { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_job_id { get; set; }
    }
}