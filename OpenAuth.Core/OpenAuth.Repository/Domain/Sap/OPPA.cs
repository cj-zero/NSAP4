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
    [Table("OPPA")]
    public partial class OPPA : Entity
    {
        public OPPA()
        {
          this.SecLevel= string.Empty;
          this.PwdExample= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SecLevel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? PwdExp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? PwdMinLen { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? MinUppers { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? MinLowCase { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? MinDigits { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? MinNonAlph { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? NumPrevPwd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? NumAuthLoc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PwdExample { get; set; }
    }
}