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
    [Table("IMPLG")]
    public partial class IMPLG : Entity
    {
        public IMPLG()
        {
          this.Owner= string.Empty;
          this.From= string.Empty;
          this.To= string.Empty;
          this.Msg= string.Empty;
          this.Time= DateTime.Now;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Owner { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string From { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string To { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Msg { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? Time { get; set; }
    }
}