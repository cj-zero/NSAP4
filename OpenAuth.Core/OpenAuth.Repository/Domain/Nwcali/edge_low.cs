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
    [Table("edge_low")]
    public partial class edge_low : BaseEntity<long>
    {
        public edge_low()
        {
          this.edge_guid= string.Empty;
          this.srv_guid= string.Empty;
          this.mid_guid= string.Empty;
          this.low_guid= string.Empty;
          this.range_volt= string.Empty;
          this.range_curr_array= string.Empty;
          this.low_version= string.Empty;
        }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string edge_guid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string srv_guid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string mid_guid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string low_guid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? low_no { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? unit_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string range_volt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string range_curr_array { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string low_version { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public ushort? status { get; set; }
        public override void GenerateDefaultKeyVal()
        {
        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}