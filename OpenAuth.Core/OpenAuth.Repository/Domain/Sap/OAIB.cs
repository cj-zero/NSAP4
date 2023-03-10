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

namespace OpenAuth.Repository.Domain.Sap
{
    /// <summary>
	/// 
	/// </summary>
    [Table("OAIB")]
    public partial class OAIB : Entity
    {
        public OAIB()
        {
          this.Opened= string.Empty;
          this.RecDate= DateTime.Now;
          this.WasRead= string.Empty;
          this.Deleted= string.Empty;
          this.Failed= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Opened { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? RecDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? RecTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string WasRead { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Deleted { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Failed { get; set; }
    }
}