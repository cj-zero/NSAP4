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
    [Table("FRC1")]
    public partial class FRC1 : Entity
    {
        public FRC1()
        {
          this.CalcMethod= string.Empty;
          this.PrcCode= string.Empty;
          this.CalMethod2= string.Empty;
          this.CalMethod3= string.Empty;
          this.Linked= string.Empty;
          this.Sign= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? CFWId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CalcMethod { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? SlpCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PrcCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CalMethod2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CalMethod3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Linked { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Sign { get; set; }
    }
}