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
    [Table("OUWTX")]
    public partial class OUWTX : Entity
    {
        public OUWTX()
        {
          this.SrcObjType= string.Empty;
          this.SrcObjAbs= 0;
          this.Cancelled= string.Empty;
          this.FullCopied= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SrcObjType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int SrcObjAbs { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Cancelled { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FullCopied { get; set; }
    }
}