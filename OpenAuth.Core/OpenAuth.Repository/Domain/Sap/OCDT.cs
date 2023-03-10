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
    [Table("OCDT")]
    public partial class OCDT : Entity
    {
        public OCDT()
        {
          this.Name= string.Empty;
          this.TERM_TYPE= string.Empty;
          this.DataSource= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string TERM_TYPE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? After_Days { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? After_Mnth { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? Day_From1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? Day_To1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? Pay_Day1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? Pay_Month1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? Day_From2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? Day_To2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? Pay_Day2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? Pay_Month2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? Day_From3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? Day_To3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? Pay_Day3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? Pay_Month3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? Day_From4 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? Day_To4 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? Pay_Day4 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? Pay_Month4 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DataSource { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign { get; set; }
    }
}