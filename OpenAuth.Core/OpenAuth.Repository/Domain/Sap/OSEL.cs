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
    [Table("OSEL")]
    public partial class OSEL : Entity
    {
        public OSEL()
        {
          this.FilterName= string.Empty;
          this.FormNum= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FilterName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FormNum { get; set; }
    }
}