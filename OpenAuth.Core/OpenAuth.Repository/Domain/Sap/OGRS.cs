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
    [Table("OGRS")]
    public partial class OGRS : Entity
    {
        public OGRS()
        {
          this.PeriodCat= string.Empty;
          this.FinancYear= DateTime.Now;
          this.PeriodName= string.Empty;
          this.SubType= string.Empty;
          this.F_RefDate= DateTime.Now;
          this.T_RefDate= DateTime.Now;
          this.F_DueDate= DateTime.Now;
          this.T_DueDate= DateTime.Now;
          this.F_TaxDate= DateTime.Now;
          this.T_TaxDate= DateTime.Now;
          this.UpdateDate= DateTime.Now;
          this.ResCode= string.Empty;
          this.WhsCode= string.Empty;
          this.LicTradNum= string.Empty;
          this.ShipCountr= string.Empty;
          this.ShipState= string.Empty;
          this.Comments= string.Empty;
          this.CreateDate= DateTime.Now;
          this.RuleCode= string.Empty;
          this.GLMethod= string.Empty;
          this.Transfered= string.Empty;
          this.FromDate= DateTime.Now;
          this.ToDate= DateTime.Now;
          this.ResRevAct= string.Empty;
          this.ResExpAct= string.Empty;
          this.ResSaleAct= string.Empty;
          this.ResPurAct= string.Empty;
          this.ResNInvAct= string.Empty;
          this.ResStdExp1= string.Empty;
          this.ResStdExp2= string.Empty;
          this.ResStdExp3= string.Empty;
          this.ResStdExp4= string.Empty;
          this.ResStdExp5= string.Empty;
          this.ResStdExp6= string.Empty;
          this.ResStdExp7= string.Empty;
          this.ResStdExp8= string.Empty;
          this.ResStdExp9= string.Empty;
          this.ResStdEx10= string.Empty;
          this.ResWipAct= string.Empty;
          this.ResScrapAc= string.Empty;
          this.WipOffPlAc= string.Empty;
          this.ResOffPlAc= string.Empty;
          this.Active= string.Empty;
          this.CmpPrivate= string.Empty;
          this.VatGroup= string.Empty;
          this.CardCode= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PeriodCat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? FinancYear { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? Year { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PeriodName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SubType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? PeriodNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? F_RefDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? T_RefDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? F_DueDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? T_DueDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? F_TaxDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? T_TaxDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? LogInstanc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? UpdateDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ResCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? ResGrpCod { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string WhsCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? BPGrpCod { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string LicTradNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ShipCountr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ShipState { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Comments { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CreateDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RuleCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string GLMethod { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Transfered { get; set; }
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
        public string ResRevAct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ResExpAct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ResSaleAct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ResPurAct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ResNInvAct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ResStdExp1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ResStdExp2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ResStdExp3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ResStdExp4 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ResStdExp5 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ResStdExp6 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ResStdExp7 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ResStdExp8 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ResStdExp9 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ResStdEx10 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ResWipAct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ResScrapAc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string WipOffPlAc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ResOffPlAc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Active { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CmpPrivate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string VatGroup { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CardCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? Usage { get; set; }
    }
}