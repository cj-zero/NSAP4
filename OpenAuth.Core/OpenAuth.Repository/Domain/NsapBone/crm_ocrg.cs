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
    [Table("crm_ocrg")]
    public partial class crm_ocrg 
    {
        public crm_ocrg()
        {
          this.GroupName= string.Empty;
          this.GroupType= string.Empty;
          this.Locked= string.Empty;
          this.DataSource= string.Empty;
          this.U_PRX_SID= string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int GroupCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int sbo_id { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string GroupName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string GroupType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Locked { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DataSource { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_PRX_SID { get; set; }
    }
}