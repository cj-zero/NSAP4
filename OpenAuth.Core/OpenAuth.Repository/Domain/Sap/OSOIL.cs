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
    [Table("OSOIL")]
    public partial class OSOIL : Entity
    {
        public OSOIL()
        {
          this.AbsEntry= 0;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? WizardId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? SOINum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int AbsEntry { get; set; }
    }
}