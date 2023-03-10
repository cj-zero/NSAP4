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
    [Table("ORTM")]
    public partial class ORTM : Entity
    {
        public ORTM()
        {
          this.Rtmdate= DateTime.Now;
          this.AcctCode= string.Empty;
          this.IsCard= string.Empty;
          this.ActCurrncy= string.Empty;
          this.Valid= string.Empty;
          this.StornoDate= DateTime.Now;
          this.SCAdjust= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? Rtmdate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AcctCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IsCard { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ActCurrncy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? ActRate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Balance { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? FrnBlnc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? TransNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Valid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? Delta { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? StornoDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? RevalRate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? BPLId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? CntBlnc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? TransAmtSC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? BalDueSC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SCAdjust { get; set; }
    }
}