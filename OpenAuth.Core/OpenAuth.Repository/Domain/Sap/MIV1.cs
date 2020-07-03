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
    [Table("MIV1")]
    public partial class MIV1 : Entity
    {
        public MIV1()
        {
          this.DocDate= DateTime.Now;
          this.DocDueDate= DateTime.Now;
          this.DocClsDate= DateTime.Now;
          this.Closed= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? MinvNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? DocNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? DocDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? DocDueDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? DocClsDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? DocAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? DocExpense { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? DocDiscSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? DocRound { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? DocTax { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Closed { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? MINumWnCls { get; set; }
    }
}