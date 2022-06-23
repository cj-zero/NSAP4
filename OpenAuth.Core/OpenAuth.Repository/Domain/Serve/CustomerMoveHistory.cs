using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Serve
{
    /// <summary>
    /// 客户掉入公海记录表
    /// </summary>
    [Table("customer_move_history")]
    public class CustomerMoveHistory : BaseEntity<int>
    {
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CardCode { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string CardName { get; set; }

        /// <summary>
        /// 原归属人销售代码
        /// </summary>
        public int SlpCode { get; set; }

        /// <summary>
        /// 原归属人
        /// </summary>
        public string SlpName { get; set; }

        /// <summary>
        /// 掉入方式:主动移入,按规则掉入
        /// </summary>
        [Column("movein_type")]
        public string MoveInType { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public string UpdateUser { get; set; }
        public override void GenerateDefaultKeyVal()
        {
        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
