/*
 * @author : wangying
 * @date : 2022-8-9
 * @desc : 工程部物料设计
 */
using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 工程部物料设计
    /// </summary>
    [Table("manage_screening_history")]
    public class ManageScreeningHistory : BaseEntity<int>
    {
        /// <summary>
        /// 单据编码
        /// </summary>
        public string DocEntry { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string ItemDesc { get; set; }
        /// <summary>
        /// 业务员编码
        /// </summary>
        public int SlpCode { get; set; }

        /// <summary>
        /// 提交时间
        /// </summary>
        public DateTime SubmitTime { get; set; }
        /// <summary>
        /// 合约评审单号
        /// </summary>
        public string ContractReviewCode { get; set; }
        /// <summary>
        /// 配置类型
        /// </summary>
        public string U_ZS { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
        public override void GenerateDefaultKeyVal()
        {
        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
