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
    [Table("TPW1")]
    public partial class TPW1 : Entity
    {
        public TPW1()
        {
          this.FromPeriod= DateTime.Now;
          this.ToPeriod= DateTime.Now;
          this.FromDate= DateTime.Now;
          this.ToDate= DateTime.Now;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? FromPeriod { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? ToPeriod { get; set; }
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
        public int? TaxCategry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? LocCode { get; set; }
    }
}