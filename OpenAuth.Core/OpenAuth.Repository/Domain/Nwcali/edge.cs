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

namespace OpenAuth.Repository.Domain
{
    /// <summary>
	/// 
	/// </summary>
    [Table("edge")]
    public partial class edge : BaseEntity<long>
    {
        public edge()
        {
          this.edg_name= string.Empty;
          this.address= string.Empty;
          this.department= string.Empty;
        }

        public string edge_guid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string edg_name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string address { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string department { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public ushort status { get; set; }
        public DateTime CreateTime { get; set; }
        public override void GenerateDefaultKeyVal()
        {
        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}