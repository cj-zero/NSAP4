using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Customer
{
    [Table("Customer_Saler_History")]
    public class CustomerSalerHistory : BaseEntity
    {
        /// <summary>
        /// 自增主键
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 客户代码
        /// </summary>
        public string CustomerNo { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// 销售员3.0代码
        /// </summary>
        public int SlpCode { get; set; }

        /// <summary>
        /// 销售员名称
        /// </summary>
        public string SlpName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 领取时间
        /// </summary>
        public DateTime? ReceiveTime { get; set; }

        /// <summary>
        /// 释放时间
        /// </summary>
        public DateTime? ReleaseTime { get; set; }

        /// <summary>
        /// 掉入公海时间
        /// </summary>
        public DateTime? FallIntoTime { get; set; }

        /// <summary>
        /// 与SAP的ACRD表关联的字段
        /// </summary>
        public int LogInstance { get; set; }

        /// <summary>
        /// 是否分配客户销售历史记录
        /// </summary>
        [Column("Is_SaleHistory")]
        public bool IsSaleHistory { get; set; }

        /// <summary>
        /// 销售员部门
        /// </summary>
        public string SlpDepartment { get; set; }
        /// <summary>
        /// 操作人
        /// </summary>
        public string CreateUserId { get; set; }
    }
}
