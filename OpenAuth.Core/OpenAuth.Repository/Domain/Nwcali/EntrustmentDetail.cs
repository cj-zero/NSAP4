using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    [Table("entrustmentdetail")]
    public class EntrustmentDetail : Entity
    {
        public EntrustmentDetail()
        {
            this.EntrustmentId = 0;
            this.LineNum = string.Empty;
            this.ItemCode = string.Empty;
            this.ItemName = string.Empty;
            this.SerialNumber = string.Empty;
            this.Quantity = 0;
            this.Status = 0;
        }
        //public string Id { get; set; }
        public int EntrustmentId { get; set; }
        /// <summary>
        /// 行号
        /// </summary>
        public string LineNum { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 序列号
        /// </summary>
        public string SerialNumber { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// 证书状态
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 排序号
        /// </summary>
        public int? Sort { get; set; }
    }
}
