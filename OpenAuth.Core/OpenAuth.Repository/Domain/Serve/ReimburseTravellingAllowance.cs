﻿//------------------------------------------------------------------------------
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
	/// 出差补贴
	/// </summary>
    [Table("reimbursetravellingallowance")]
    public partial class ReimburseTravellingAllowance : BaseEntity<int>
    {
        public ReimburseTravellingAllowance()
        {
          this.Remark= string.Empty;
        }


        /// <summary>
        /// 报销单ID
        /// </summary>
        [Description("报销单ID")]
        [Browsable(false)]
        public int ReimburseInfoId { get; set; }
        /// <summary>
        /// 序号
        /// </summary>
        [Description("序号")]
        public int? SerialNumber { get; set; }
        /// <summary>
        /// 天数
        /// </summary>
        [Description("天数")]
        public int? Days { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        [Description("金额")]
        public decimal? Money { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Description("备注")]
        public string Remark { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public DateTime? CreateTime { get; set; }

        public override void GenerateDefaultKeyVal()
        {
        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}