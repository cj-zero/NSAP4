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
    [Table("OSQR")]
    public partial class OSQR : Entity
    {
        public OSQR()
        {
          this.QName= string.Empty;
          this.QString= string.Empty;
          this.QType= string.Empty;
          this.ColumnSize= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? QCategory { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QString { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ColumnSize { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? DBType { get; set; }
    }
}