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
    [Table("OVNM")]
    public partial class OVNM : Entity
    {
        public OVNM()
        {
          this.NumName= string.Empty;
          this.First= string.Empty;
          this.Next= string.Empty;
          this.Last= string.Empty;
          this.YrDepend= string.Empty;
          this.DefaultNum= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string NumName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string First { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Next { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Last { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string YrDepend { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DefaultNum { get; set; }
    }
}