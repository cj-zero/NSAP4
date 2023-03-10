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
    [Table("OCST")]
    public partial class OCST : Entity
    {
        public OCST()
        {
          this.Name= string.Empty;
          this.GNRECode= string.Empty;
          this.GSTCode= string.Empty;
          this.GSTIsUT= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Code { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Country { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? eCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string GNRECode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string GSTCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string GSTIsUT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? GroupCode { get; set; }
    }
}