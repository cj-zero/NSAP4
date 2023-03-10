//------------------------------------------------------------------------------
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
    [Table("base_contact")]
    public partial class base_contact 
    {
        public base_contact()
        {
          this.topic= string.Empty;
          this.add_dep= string.Empty;
          this.add_dep_id= string.Empty;
          this.adapt_model= string.Empty;
          this.content= string.Empty;
          this.up_dt= DateTime.Now;
          this.ciiDate= DateTime.Now;
        }

        public int seq_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string topic { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string add_dep { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string add_dep_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string adapt_model { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? user_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public sbyte new_import { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public sbyte lib_import { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public sbyte ship_import { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public sbyte customer_need { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public sbyte rd_need { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string content { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime up_dt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? job_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? ciiDate { get; set; }
    }
}