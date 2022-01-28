using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 内联单生产订单
    /// </summary>
    [Table("internalcontactproduction")]
    public class InternalContactProduction : Entity
    {
        /// <summary>
        /// 生产订单
        /// </summary>
        public int ProductionId { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 归属量
        /// </summary>
        public int BelongQty { get; set; }
        public int? InternalContactId { get; set; }
    }
}
