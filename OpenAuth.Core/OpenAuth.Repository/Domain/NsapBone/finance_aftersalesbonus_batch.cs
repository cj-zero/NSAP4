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

namespace OpenAuth.Repository.Domain
{
    /// <summary>
	/// 
	/// </summary>
    [Table("finance_aftersalesbonus_batch")]
    public partial class finance_aftersalesbonus_batch 
    {
        public finance_aftersalesbonus_batch()
        {
          this.ExportBatchNo= string.Empty;
          this.user_id= 0;
          this.CreateTime= DateTime.Now;
          this.UpdateTime= DateTime.Now;
          this.BalComments= string.Empty;
        }

        public int? ExportBatchId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ExportBatchNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int user_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? SlpCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? ExportStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CreateTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? TotalBonus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? BalIdx { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BalComments { get; set; }
    }
}