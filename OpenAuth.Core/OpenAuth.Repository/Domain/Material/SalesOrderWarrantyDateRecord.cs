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
    [Table("salesorderwarrantydaterecord")]
    public partial class SalesOrderWarrantyDateRecord : Entity
    {
        public SalesOrderWarrantyDateRecord()
        {
          this.SalesOrderWarrantyDateId= string.Empty;
          this.Action= string.Empty;
          this.CreateUserId= string.Empty;
          this.CreateUser= string.Empty;
          this.CreateTime= DateTime.Now;
        }

        /// <summary>
        /// 销售订单id
        /// </summary>
        [Description("销售订单id")]
        public string SalesOrderWarrantyDateId { get; set; }
        /// <summary>
        /// 操作内容
        /// </summary>
        [Description("操作内容")]
        public string Action { get; set; }
        /// <summary>
        /// 创建人id
        /// </summary>
        [Description("创建人id")]
        public string CreateUserId { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        [Description("创建人")]
        public string CreateUser { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public System.DateTime? CreateTime { get; set; }
    }
}