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
    [Table("TRS1")]
    public partial class TRS1 : Entity
    {
        public TRS1()
        {
          this.ObjType= string.Empty;
          this.ApDocType= string.Empty;
          this.ApDocEntry= 0;
          this.ApDocOrdNo= 0;
          this.MeansType= 0;
          this.PayLineID= 0;
          this.PayDocType= 0;
          this.PayDocEntr= 0;
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
        public string ApDocType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int ApDocEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int ApDocOrdNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? ApDocAdRef { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int MeansType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int PayLineID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int PayDocType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int PayDocEntr { get; set; }
    }
}