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
    [Table("OBOX")]
    public partial class OBOX : Entity
    {
        public OBOX()
        {
          this.BoxName= string.Empty;
          this.BoxType= string.Empty;
          this.SummayFld= string.Empty;
          this.DbtCrdt= string.Empty;
          this.Formula= string.Empty;
          this.AbsolutVa= string.Empty;
          this.Inactive= string.Empty;
          this.EffecDate= DateTime.Now;
          this.DecLoc= string.Empty;
          this.Position= string.Empty;
          this.PostToAct= string.Empty;
          this.PostToOffA= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BoxName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BoxType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SummayFld { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DbtCrdt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Formula { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? SortOrder { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AbsolutVa { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Inactive { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? EffecDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DecLoc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Position { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PostToAct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PostToOffA { get; set; }
    }
}