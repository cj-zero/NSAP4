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
    [Table("SalesOrderWarrantyDate")]
    public partial class SalesOrderWarrantyDate : Entity
    {
        public SalesOrderWarrantyDate()
        {
          this.CustomerId= string.Empty;
          this.CustomerName= string.Empty;
          this.Remark= string.Empty;
          this.DeliveryDate= DateTime.Now;
          this.CreateTime= DateTime.Now;
        }
        /// <summary>
        /// 销售订单id
        /// </summary>
        [Description("销售订单id")]
        [Browsable(false)]
        public int? SalesOrderId { get; set; }

        

        /// <summary>
        /// 客户代码
        /// </summary>
        [Description("客户代码")]
        [Browsable(false)]
        public string CustomerId { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        [Description("客户名称")]
        public string CustomerName { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Description("备注")]
        public string Remark { get; set; }
        /// <summary>
        /// 保修开始时间
        /// </summary>
        [Description("保修开始时间")]
        public System.DateTime? DeliveryDate { get; set; }
        /// <summary>
        /// 保修结束时间
        /// </summary>
        [Description("保修结束时间")]
        public System.DateTime? WarrantyPeriod { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public System.DateTime? CreateTime { get; set; }

        /// <summary>
        /// 销售人名称
        /// </summary>
        [Description("销售人名称")]
        public string SalesOrderName { get; set; }

        /// <summary>
        /// 是否通过
        /// </summary>
        [Description("是否通过")]
        public bool? IsPass { get; set; }

        /// <summary>
        /// 序列号
        /// </summary>
        [Description("序列号")]
        public string MnfSerial { get; set; }

        /// <summary>
        /// 销售人员code
        /// </summary>
        [Description("销售人员code")]
        public int? SlpCode { get; set; }

        /// <summary>
        /// 操作记录表
        /// </summary>
        public virtual List<SalesOrderWarrantyDateRecord> SalesOrderWarrantyDateRecords { get; set; }
    }
}