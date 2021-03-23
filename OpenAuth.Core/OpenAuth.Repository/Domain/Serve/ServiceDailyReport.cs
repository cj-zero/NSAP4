using OpenAuth.Repository.Core;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
	/// 
	/// </summary>
    [Table("servicedailyreport")]
    public partial class ServiceDailyReport : Entity
    {
        public ServiceDailyReport()
        {
            this.ManufacturerSerialNumber = string.Empty;
            this.TroubleDescription = string.Empty;
            this.ProcessDescription = string.Empty;
            this.CreateUserId = string.Empty;
            this.CreaterName = string.Empty;
            this.EditTime = DateTime.Now;
            this.CreateTime = DateTime.Now;
        }


        /// <summary>
        /// 服务单Id
        /// </summary>
        [Description("服务单Id")]
        [Browsable(false)]
        public int? ServiceOrderId { get; set; }
        /// <summary>
        /// 制造商序列号
        /// </summary>
        [Description("制造商序列号")]
        public string ManufacturerSerialNumber { get; set; }
        /// <summary>
        /// 问题描述
        /// </summary>
        [Description("问题描述")]
        public string TroubleDescription { get; set; }
        /// <summary>
        /// 解决方案
        /// </summary>
        [Description("解决方案")]
        public string ProcessDescription { get; set; }
        /// <summary>
        /// 创建人Id
        /// </summary>
        [Description("创建人Id")]
        [Browsable(false)]
        public string CreateUserId { get; set; }
        /// <summary>
        /// 创建人名称
        /// </summary>
        [Description("创建人名称")]
        public string CreaterName { get; set; }
        /// <summary>
        /// 编辑时间
        /// </summary>
        [Description("编辑时间")]
        public DateTime? EditTime { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public DateTime? CreateTime { get; set; }
    }
}