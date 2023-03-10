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
    [Table("ODTP")]
    public partial class ODTP : Entity
    {
        public ODTP()
        {
          this.Descr= string.Empty;
          this.DprMeth= string.Empty;
          this.Rounding= string.Empty;
          this.InclSalv= string.Empty;
          this.PerAcq= string.Empty;
          this.PerSubAcq= string.Empty;
          this.PerRet= string.Empty;
          this.AcqPRTyp= string.Empty;
          this.SubPRTyp= string.Empty;
          this.RetPRTyp= string.Empty;
          this.ValidFrom= DateTime.Now;
          this.ValidTo= DateTime.Now;
          this.sCalcMeth= string.Empty;
          this.dBase= string.Empty;
          this.dAltDprTyp= string.Empty;
          this.maDecBase= string.Empty;
          this.spMeth= string.Empty;
          this.spAdDpr= string.Empty;
          this.spAlDpr= string.Empty;
          this.PoolID= string.Empty;
          this.DataSource= string.Empty;
          this.CreateDate= DateTime.Now;
          this.UpdateDate= DateTime.Now;
          this.DprPer= string.Empty;
          this.spMaxFlag= string.Empty;
          this.CalcBase= string.Empty;
          this.DeprEndLFY= string.Empty;
          this.AccuPriorP= string.Empty;
          this.FactorFFY= string.Empty;
          this.PerTranSou= string.Empty;
          this.PerTranTar= string.Empty;
          this.TranSPRTyp= string.Empty;
          this.TranTPRTyp= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Descr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DprMeth { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? DprTo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Rounding { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string InclSalv { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? SalvPerc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PerAcq { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PerSubAcq { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PerRet { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AcqPRTyp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SubPRTyp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RetPRTyp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? PerDpRev { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? ValidFrom { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? ValidTo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string sCalcMeth { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? sPercent { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string dBase { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? dPercent { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? dFactor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string dAltDprTyp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string maDecBase { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string spMeth { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? spConcPer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? spMaxPerc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string spAdDpr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string spAlDpr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PoolID { get; set; }
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
        public int? LogInstanc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CreateDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? UpdateDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DprPer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? PerFactor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? spMaxAmnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string spMaxFlag { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CalcBase { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DeprEndLFY { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AccuPriorP { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? DeltaCoeff { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? MaxDepr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FactorFFY { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? SnapshotId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PerTranSou { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PerTranTar { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string TranSPRTyp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string TranTPRTyp { get; set; }
    }
}