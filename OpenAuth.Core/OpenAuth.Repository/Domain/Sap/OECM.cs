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
    [Table("OECM")]
    public partial class OECM : Entity
    {
        public OECM()
        {
          this.Descr= string.Empty;
          this.IsActive= string.Empty;
          this.LHost= string.Empty;
          this.LTimeout= string.Empty;
          this.RHost= string.Empty;
          this.RTimeout= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Descr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? LogInstanc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UIOrder { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? StrIndex { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IsActive { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string LHost { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? LPID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string LTimeout { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RHost { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? RPID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RTimeout { get; set; }
    }
}