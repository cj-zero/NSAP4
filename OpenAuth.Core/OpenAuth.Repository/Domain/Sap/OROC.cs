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
    [Table("OROC")]
    public partial class OROC : Entity
    {
        public OROC()
        {
          this.BoeStatus= string.Empty;
          this.Descript= string.Empty;
          this.FileFormat= string.Empty;
          this.BankCode= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? OccurCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? MovemnCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BoeStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Descript { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? Color { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FileFormat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BankCode { get; set; }
    }
}