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
    [Table("AWEX")]
    public partial class AWEX : Entity
    {
        public AWEX()
        {
          this.ProcInstId= string.Empty;
          this.BizKey= string.Empty;
          this.ActId= string.Empty;
          this.DataContex= string.Empty;
          this.LastUpdate= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? IsActive { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? IsConCurr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? IsScope { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string ProcInstId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BizKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? ParentId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? ProcDefId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string ActId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DataContex { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? B1WFInstId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string LastUpdate { get; set; }
    }
}