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
    [Table("AWMG")]
    public partial class AWMG : Entity
    {
        public AWMG()
        {
          this.TmplateKey= string.Empty;
          this.Name= string.Empty;
          this.Version= string.Empty;
          this.Status= string.Empty;
          this.Desc= string.Empty;
          this.StartType= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? TemplateID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string TmplateKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Version { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? MAXIns { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.Byte[] XMLFile { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Desc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string StartType { get; set; }
    }
}