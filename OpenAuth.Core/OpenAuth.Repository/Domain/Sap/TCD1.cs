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
    [Table("TCD1")]
    public partial class TCD1 : Entity
    {
        public TCD1()
        {
          this.TcdId= 0;
          this.Priority= 0;
          this.Descr= string.Empty;
          this.UDFTable_1= string.Empty;
          this.UDFAlias_1= string.Empty;
          this.UDFTable_2= string.Empty;
          this.UDFAlias_2= string.Empty;
          this.UDFTable_3= string.Empty;
          this.UDFAlias_3= string.Empty;
          this.UDFTable_4= string.Empty;
          this.UDFAlias_4= string.Empty;
          this.UDFTable_5= string.Empty;
          this.UDFAlias_5= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int TcdId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? KeyFld_1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? KeyFld_2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? KeyFld_3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int Priority { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Descr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? KeyFld_4 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string UDFTable_1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string UDFAlias_1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string UDFTable_2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string UDFAlias_2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string UDFTable_3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string UDFAlias_3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string UDFTable_4 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string UDFAlias_4 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? KeyFld_5 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string UDFTable_5 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string UDFAlias_5 { get; set; }
    }
}