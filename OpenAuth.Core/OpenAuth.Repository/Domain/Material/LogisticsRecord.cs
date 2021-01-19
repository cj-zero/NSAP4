using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Material
{
    /// <summary>
    /// 
    /// </summary>
    [Table("logisticsrecord")]
    public class LogisticsRecord : Entity
    {

        public LogisticsRecord()
        {
           
        }
        /// <summary>
        /// 物料单id
        /// </summary>
        [Description("物料单id")]
        public int? QuotationId { get; set; }

        /// <summary>
        /// 物料id
        /// </summary>
        [Description("物料id")]
        public string QuotationMaterialId { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        [Description("数量")]
        public int? Quantity { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Description("创建人")]
        public DateTime? CreateUser { get; set; }

        /// <summary>
        /// 创建人id
        /// </summary>
        [Description("创建人id")]
        public DateTime? CreateUserId { get; set; }

    }
}
