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
    [Table("wfa_eshop_oqutdetail")]
    public partial class wfa_eshop_oqutdetail : BaseEntity<int>
    {
        public wfa_eshop_oqutdetail()
        {
          this.document_id= 0;
          this.item_code= string.Empty;
          this.item_desc= string.Empty;
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
        public string item_code { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string item_desc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? item_qty { get; set; }
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