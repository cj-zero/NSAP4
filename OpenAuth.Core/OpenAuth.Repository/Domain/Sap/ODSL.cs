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
    [Table("ODSL")]
    public partial class ODSL : Entity
    {
        public ODSL()
        {
          this.ObjType= string.Empty;
          this.DocEntry= 0;
          this.DocLineNum= 0;
          this.SchdLine= 0;
          this.ItemCode= string.Empty;
          this.WhsCode= string.Empty;
          this.CfmDate= DateTime.Now;
          this.FixedCfm= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ObjType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int DocEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int DocLineNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int SchdLine { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ItemCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string WhsCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CfmDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? CfmQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FixedCfm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? ReqQty { get; set; }
    }
}