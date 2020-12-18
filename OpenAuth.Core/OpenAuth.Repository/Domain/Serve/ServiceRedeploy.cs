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
    [Table("serviceredeploy")]
    public partial class ServiceRedeploy : Entity
    {
        public ServiceRedeploy()
        {
            this.MaterialType = string.Empty;
            this.CreateTime = DateTime.Now;
        }


        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
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
        [Browsable(false)]
        public int? TechnicianId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CreateTime { get; set; }

        [Description("")]
        public string WorkOrderIds { get; set; }

    }
}