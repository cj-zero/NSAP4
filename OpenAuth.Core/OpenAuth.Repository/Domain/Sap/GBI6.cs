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
    [Table("GBI6")]
    public partial class GBI6 : Entity
    {
        public GBI6()
        {
          this.AcctCode= string.Empty;
          this.Currency= string.Empty;
          this.EvalGrp= string.Empty;
          this.AcctPeriod= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AcctCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Currency { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EvalGrp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? PerBBal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? PerBQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? PerBFCBal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? CPDAmt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? CPDQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? CPDFCAmt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? CPCAmt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? CPCQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? CPCFCAmt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? PerEBal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? PerEQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? PerEFCBal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AcctPeriod { get; set; }
    }
}