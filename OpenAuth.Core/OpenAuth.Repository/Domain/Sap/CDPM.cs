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
    [Table("CDPM")]
    public partial class CDPM : Entity
    {
        public CDPM()
        {
          this.Name= string.Empty;
          this.ObjectKey= string.Empty;
          this.System= string.Empty;
          this.Hidden= string.Empty;
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
        public int? ObjectType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ObjectKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? Father { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? PermOption { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string System { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Hidden { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? SortOrder { get; set; }
    }
}