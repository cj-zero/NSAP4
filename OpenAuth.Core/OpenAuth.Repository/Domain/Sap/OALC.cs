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
    [Table("OALC")]
    public partial class OALC : Entity
    {
        public OALC()
        {
          this.AlcName= string.Empty;
          this.OhType= string.Empty;
          this.AcctCode= string.Empty;
          this.DataSource= string.Empty;
          this.LaCAllcAcc= string.Empty;
          this.CostCateg= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AlcName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string OhType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AcctCode { get; set; }
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
        public string LaCAllcAcc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CostCateg { get; set; }
    }
}