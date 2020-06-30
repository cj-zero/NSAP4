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
    [Table("AEC3")]
    public partial class AEC3 : Entity
    {
        public AEC3()
        {
          this.LogType= string.Empty;
          this.LogMessage= string.Empty;
          this.LogData= string.Empty;
          this.LogOpDate= DateTime.Now;
          this.CreateDate= DateTime.Now;
          this.UpdateDate= DateTime.Now;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string LogType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string LogMessage { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string LogData { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? LogOpDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? LogOpTS { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? ExportFmt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CreateDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? CreateTS { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? UpdateDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? UpdateTS { get; set; }
    }
}