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
    [Table("GDP2")]
    public partial class GDP2 : Entity
    {
        public GDP2()
        {
          this.NatObjType= string.Empty;
          this.NatObjArr= 0;
          this.NatObjKey1= string.Empty;
          this.NatObjKey2= string.Empty;
          this.ErrorStr= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string NatObjType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int NatObjArr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string NatObjKey1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string NatObjKey2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? Result { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ErrorStr { get; set; }
    }
}