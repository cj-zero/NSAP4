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
    [Table("OZRD")]
    public partial class OZRD : Entity
    {
        public OZRD()
        {
          this.DocDate= DateTime.Now;
          this.POSEquipNo= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? DocDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string POSEquipNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? ResetCntr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? SummaryCnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? OperCntr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? TotalSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? GrossSale { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? PISSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? COFINSSum { get; set; }
    }
}