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
    [Table("CSPI")]
    public partial class CSPI : Entity
    {
        public CSPI()
        {
          this.Name= string.Empty;
          this.Type= string.Empty;
          this.Value= string.Empty;
          this.LstUpdDate= DateTime.Now;
          this.CheckSum= string.Empty;
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
        public string Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Value { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? LstUpdDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? LstUpdTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CheckSum { get; set; }
    }
}