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
    [Table("ACL1")]
    public partial class ACL1 : Entity
    {
        public ACL1()
        {
          this.Date= DateTime.Now;
          this.Location= string.Empty;
          this.Latitude= string.Empty;
          this.Longitude= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? Date { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Location { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Latitude { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Longitude { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? Time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? OwnerUser { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? OwnerEmp { get; set; }
    }
}