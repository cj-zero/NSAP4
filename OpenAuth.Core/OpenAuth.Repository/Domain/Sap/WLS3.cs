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
    [Table("WLS3")]
    public partial class WLS3 : Entity
    {
        public WLS3()
        {
          this.Note= string.Empty;
          this.Creator= string.Empty;
          this.NoteDate= DateTime.Now;
          this.Access= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? WFInstID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Note { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Creator { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? NoteDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Access { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? LogIns { get; set; }
    }
}