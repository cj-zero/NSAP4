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
    [Table("wfa_eshop_canceledstatus")]
    public partial class wfa_eshop_canceledstatus : BaseEntity<int>
    {
        public wfa_eshop_canceledstatus()
        {
          this.job_id= 0;
        }

        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int document_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int job_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? cur_status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? quotation_entry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? order_entry { get; set; }

        public virtual wfa_eshop_status wfa_Eshop_Status { get; set; }
        public override void GenerateDefaultKeyVal()
        {
        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}