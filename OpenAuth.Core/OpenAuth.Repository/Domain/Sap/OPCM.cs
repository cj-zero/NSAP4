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
    [Table("OPCM")]
    public partial class OPCM : Entity
    {
        public OPCM()
        {
          this.IdCode= string.Empty;
          this.PosCode= string.Empty;
          this.PosDesc= string.Empty;
          this.CrCode= string.Empty;
          this.CrDesc= string.Empty;
          this.Remarks= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string IdCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PosCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PosDesc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CrCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CrDesc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Remarks { get; set; }
    }
}