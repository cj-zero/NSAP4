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
    [Table("OBBI")]
    public partial class OBBI : Entity
    {
        public OBBI()
        {
          this.TableCode= string.Empty;
          this.BrandCode= 0;
          this.GroupCode= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string TableCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int BrandCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string GroupCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign { get; set; }
    }
}