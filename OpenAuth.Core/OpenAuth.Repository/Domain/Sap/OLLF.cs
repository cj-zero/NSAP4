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
    [Table("OLLF")]
    public partial class OLLF : Entity
    {
        public OLLF()
        {
          this.Name= string.Empty;
          this.Descr= string.Empty;
          this.MenuGuid= string.Empty;
          this.Version= string.Empty;
          this.SchVersion= string.Empty;
          this.OutPath= string.Empty;
          this.MenuName= string.Empty;
          this.MenuPath= string.Empty;
          this.Assigned= string.Empty;
          this.SboVersion= string.Empty;
          this.Type= string.Empty;
          this.SubType= string.Empty;
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
        public string Descr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string MenuGuid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Version { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SchVersion { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string OutPath { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? FrmId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? UpdateNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string MenuName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string MenuPath { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Assigned { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SboVersion { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SubType { get; set; }
    }
}