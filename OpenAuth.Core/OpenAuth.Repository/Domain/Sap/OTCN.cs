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
    [Table("OTCN")]
    public partial class OTCN : Entity
    {
        public OTCN()
        {
          this.CCDNum= string.Empty;
          this.Date= DateTime.Now;
          this.CustTerm= string.Empty;
          this.CntrOrigin= string.Empty;
          this.DirectImp= string.Empty;
          this.CardCode= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CCDNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? Date { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CustTerm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CntrOrigin { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DirectImp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CardCode { get; set; }
    }
}