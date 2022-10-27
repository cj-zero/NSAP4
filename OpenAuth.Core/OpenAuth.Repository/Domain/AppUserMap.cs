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
    [Table("appusermap")]
    public partial class AppUserMap : Entity
    {
        public AppUserMap()
        {
          this.UserID= string.Empty;
        }

        
        /// <summary>
        /// NSAP的UserId
        /// </summary>
        [Description("NSAP的UserId")]
        public string UserID { get; set; }
        /// <summary>
        /// NSAP的User
        /// </summary>
        public virtual User User { get; set; }
        /// <summary>
        /// app的UserId
        /// </summary>
        [Description("App的UserId")]
        [Browsable(false)]
        public int? AppUserId { get; set; }
        /// <summary>
        /// App的UserRole
        /// </summary>
        [Description("App的UserRole")]
        public int? AppUserRole { get; set; }

        /// <summary>
        /// App的UserRole
        /// </summary>
        [Description("PassPort的PassPortId")]
        public int? PassPortId { get; set; }




    }
}