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
    [Table("ICD6")]
    public partial class ICD6 : Entity
    {
        public ICD6()
        {
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? UomQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? InvtQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? LogIns { get; set; }
    }
}