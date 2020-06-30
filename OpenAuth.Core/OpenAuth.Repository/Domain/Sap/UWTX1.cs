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

namespace OpenAuth.Repository.Domain.Sap
{
    /// <summary>
	/// 
	/// </summary>
    [Table("UWTX1")]
    public partial class UWTX1 : Entity
    {
        public UWTX1()
        {
          this.BaseObjTyp= string.Empty;
          this.WTaxAbsId= string.Empty;
          this.Account= string.Empty;
          this.BaseType= string.Empty;
          this.CrditDebit= string.Empty;
          this.PostingTyp= string.Empty;
          this.Category= string.Empty;
          this.WhtType= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? SrcArrType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? SrcLineNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? SrcGrpNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BaseObjTyp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? BaseAbs { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? BaseArrTyp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? BaseLinNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? BaseGrpNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string WTaxAbsId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Account { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Rate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Exemption { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BaseType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? BaseNetSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? BaseNetSC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? BaseNetFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? BaseVatSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? BaseVatSC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? BaseVatFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? AccBaseSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? AccBaseSC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? AccBaseFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? AccWTaxSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? AccWTaxSC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? AccWTaxFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? TxblSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? TxblSumSc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? TxblSumFc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? WTaxSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? WTaxSumSc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? WTaxSumFc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? ApplSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? ApplSumSc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? ApplSumFc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CrditDebit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PostingTyp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Category { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? WTTypeId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string WhtType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? FmlId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? BaseMin { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? BaseMinSc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? BaseMinFc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? ResMin { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? ResMinSc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? ResMinFc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? AddBas { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? AddBasSc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? AddBasFc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? ABWVAT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? ABWVATSc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? ABWVATFc { get; set; }
    }
}