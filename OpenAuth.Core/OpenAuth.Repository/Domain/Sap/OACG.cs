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
    [Table("OACG")]
    public partial class OACG : Entity
    {
        public OACG()
        {
          this.Name= string.Empty;
          this.Source= string.Empty;
          this.Locked= string.Empty;
          this.DateSource= string.Empty;
          this.UserSign= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Source { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Locked { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DateSource { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string UserSign { get; set; }
    }
}