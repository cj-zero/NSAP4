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
    [Table("OIST")]
    public partial class OIST : Entity
    {
        public OIST()
        {
          this.InstrCode= string.Empty;
          this.InstrDespt= string.Empty;
          this.IsCancel= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string InstrCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string InstrDespt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IsCancel { get; set; }
    }
}