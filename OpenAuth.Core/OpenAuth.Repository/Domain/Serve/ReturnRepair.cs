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
    [Table("returnrepair")]
    public partial class ReturnRepair : Entity
    {
        public ReturnRepair()
        {
            this.ServiceOrderId = 0;
            this.ServiceOrderSapId = 0;
            this.MaterialType = string.Empty;
            this.FlowInstanceId = string.Empty;
            this.CreateTime = DateTime.Now;
            this.CreateUserId = string.Empty;
            this.CreateUser = string.Empty;
        }



        /// <summary>
        ///
        /// </summary>
        [Description("")]
        public int Id { get; set; }

        /// <summary>
        /// 服务单主键Id
        /// </summary>
        [Description("服务单主键Id")]
        [Browsable(false)]
        public int ServiceOrderId { get; set; }
        /// <summary>
        /// SAP服务Id
        /// </summary>
        [Description("SAP服务Id")]
        [Browsable(false)]
        public int ServiceOrderSapId { get; set; }
        /// <summary>
        /// 设备类型
        /// </summary>
        [Description("设备类型")]
        public string MaterialType { get; set; }
        /// <summary>
        /// 工作流程Id
        /// </summary>
        [Description("工作流程Id")]
        [Browsable(false)]
        public string FlowInstanceId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public System.DateTime CreateTime { get; set; }
        /// <summary>
        /// 创建人Id
        /// </summary>
        [Description("创建人Id")]
        [Browsable(false)]
        public string CreateUserId { get; set; }
        /// <summary>
        /// 创建人名
        /// </summary>
        [Description("创建人名")]
        public string CreateUser { get; set; }

        /// <summary>
        /// 物流表
        /// </summary>
        public virtual List<Express> Express { get; set; }
    }
}