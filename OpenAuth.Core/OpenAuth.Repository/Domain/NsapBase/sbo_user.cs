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
    [Table("sbo_user")]
    public partial class sbo_user 
    {
        public sbo_user()
        {
          this.upd_dt= DateTime.Now;
        }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public uint sbo_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public uint user_id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? sale_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public uint tech_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime upd_dt { get; set; }
    }
}