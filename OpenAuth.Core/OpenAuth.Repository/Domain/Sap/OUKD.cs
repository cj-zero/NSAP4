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
    [Table("OUKD")]
    public partial class OUKD : Entity
    {
        public OUKD()
        {
          this.KeyName= string.Empty;
          this.UniqueKey= string.Empty;
          this.Action= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string KeyName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string UniqueKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Action { get; set; }
    }
}