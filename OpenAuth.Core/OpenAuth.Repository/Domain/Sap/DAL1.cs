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
    [Table("DAL1")]
    public partial class DAL1 : Entity
    {
        public DAL1()
        {
          this.ItemID= string.Empty;
          this.ObjName= string.Empty;
          this.PropName= string.Empty;
          this.QueryCol= string.Empty;
          this.MobDesc= string.Empty;
          this.IsUDF= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? LinkEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ItemID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ObjName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PropName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string QueryCol { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string MobDesc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IsUDF { get; set; }
    }
}