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
    [Table("MTH3V")]
    public partial class MTH3V : Entity
    {
        public MTH3V()
        {
          this.AcctCdeFrm= string.Empty;
          this.AcctCdeTo= string.Empty;
          this.IsCard= string.Empty;
          this.MthDateFrm= DateTime.Now;
          this.MthDateTo= DateTime.Now;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? absEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AcctCdeFrm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AcctCdeTo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IsCard { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? MatchNumFr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? MatchNumTo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? MthDateFrm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? MthDateTo { get; set; }
    }
}