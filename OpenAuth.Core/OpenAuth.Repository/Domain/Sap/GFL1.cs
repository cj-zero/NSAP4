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
    [Table("GFL1")]
    public partial class GFL1 : Entity
    {
        public GFL1()
        {
          this.Value= string.Empty;
          this.ValueTo= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? FilterRule { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Value { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ValueTo { get; set; }
    }
}