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
    [Table("GDP3")]
    public partial class GDP3 : Entity
    {
        public GDP3()
        {
          this.RefObjType= string.Empty;
          this.RefObjKey1= string.Empty;
          this.RefObjKey2= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RefObjType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RefObjKey1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RefObjKey2 { get; set; }
    }
}