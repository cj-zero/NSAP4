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
    [Table("BNK1")]
    public partial class BNK1 : Entity
    {
        public BNK1()
        {
          this.DocID= string.Empty;
          this.IsDebit= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DocID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? AmntLC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? AmnFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IsDebit { get; set; }
    }
}