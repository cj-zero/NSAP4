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

namespace OpenAuth.Repository.Domain
{
    /// <summary>
	/// 
	/// </summary>
    [Table("serviceflow")]
    public partial class ServiceFlow : Entity
    {
        public ServiceFlow()
        {
            this.MaterialType = string.Empty;
            this.FlowName = string.Empty;
            this.Creater = string.Empty;
            this.CreaterName = string.Empty;
            this.CreateTime = DateTime.Now;
        }



        [Description("")]
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? ServiceOrderId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string MaterialType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? FlowType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? FlowNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FlowName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? IsProceed { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Creater { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CreaterName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CreateTime { get; set; }
    }
}