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
    [Table("PMG2")]
    public partial class PMG2 : Entity
    {
        public PMG2()
        {
          this.REMARKS= string.Empty;
          this.CLOSED= string.Empty;
          this.SOLUTION= string.Empty;
          this.DATE= DateTime.Now;
          this.EncryptIV= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? StageID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? AREA { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? PRIORITY { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string REMARKS { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CLOSED { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? SOLUTIONID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SOLUTION { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? RESPNSIBLE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? ENTERED { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? DATE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? EFFORT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? LogInstanc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EncryptIV { get; set; }
    }
}