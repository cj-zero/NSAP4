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
    [Table("OHMM")]
    public partial class OHMM : Entity
    {
        public OHMM()
        {
          this.ModelAuth= string.Empty;
          this.ModelName= string.Empty;
          this.ModelVer= string.Empty;
          this.Desc= string.Empty;
          this.Status= string.Empty;
          this.ChangeBy= string.Empty;
          this.CreateDate= string.Empty;
          this.ChangeDate= string.Empty;
          this.DeployDate= DateTime.Now;
          this.Language= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ModelAuth { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ModelName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ModelVer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Desc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.Byte[] InfoFile { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? UpdateBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ChangeBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CreateDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? CreateTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ChangeDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? ChangeTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? TaskId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? DeployDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? DeployTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Language { get; set; }
    }
}