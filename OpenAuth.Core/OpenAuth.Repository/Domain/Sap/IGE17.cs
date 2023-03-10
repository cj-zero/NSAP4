//------------------------------------------------------------------------------
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
    [Table("IGE17")]
    public partial class IGE17 : Entity
    {
        public IGE17()
        {
          this.ObjectType= string.Empty;
          this.ImpDocType= string.Empty;
          this.ImpDocNum= string.Empty;
          this.DateOfReg= DateTime.Now;
          this.CustClrDat= DateTime.Now;
          this.ConcActNum= string.Empty;
          this.AdditNum= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? LogInstanc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ObjectType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ImpDocType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ImpDocNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? DateOfReg { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CustClrDat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ConcActNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AdditNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? AddItmDV { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? tpVTransp { get; set; }
    }
}