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
    [Table("OTRT")]
    public partial class OTRT : Entity
    {
        public OTRT()
        {
          this.Dscription= string.Empty;
          this.FrgnMode= string.Empty;
          this.Memo= string.Empty;
          this.TransCode= string.Empty;
          this.DataSource= string.Empty;
          this.StampTax= string.Empty;
          this.AutoVat= string.Empty;
          this.ManageWTax= string.Empty;
          this.DeferedTax= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Dscription { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FrgnMode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Memo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string TransCode { get; set; }
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
        public string StampTax { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AutoVat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ManageWTax { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DeferedTax { get; set; }
    }
}