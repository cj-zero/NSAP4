﻿//------------------------------------------------------------------------------
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
    [Table("OSRC")]
    public partial class OSRC : Entity
    {
        public OSRC()
        {
          this.SysRptName= string.Empty;
          this.CusRptName= string.Empty;
          this.RptChoice= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SysRptName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.Byte[] SysRptTemp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CusRptName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.Byte[] CusRptTemp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RptChoice { get; set; }
    }
}