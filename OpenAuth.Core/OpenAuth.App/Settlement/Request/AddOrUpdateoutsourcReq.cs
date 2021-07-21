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
using Infrastructure.AutoMapper;
using OpenAuth.Repository.Core;
using OpenAuth.Repository.Domain.Settlement;

namespace OpenAuth.App.Request
{
    /// <summary>
	/// 
	/// </summary>
    [Table("Outsourc")]
    [AutoMapTo(typeof(Outsourc))]
    public partial class AddOrUpdateoutsourcReq 
    {
        /// <summary>
        /// 结算id
        /// </summary>
        public int? outsourcId { get; set; }
        /// <summary>
        /// 支付时间
        /// </summary>
        public System.DateTime? PayTime { get; set; }
        /// <summary>
        /// 总金额
        /// </summary>
        public decimal? TotalMoney { get; set; }
        /// <summary>
        /// 结算方式
        /// </summary>
        public int? ServiceMode { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 创建人姓名
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 创建人id
        /// </summary>
        public string CreateUserId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public System.DateTime? CreateTime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public System.DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 是否草稿状态
        /// </summary>
        public bool IsDraft { get; set; }
        /// <summary>
        /// 服务单id
        /// </summary>
        public int? ServiceOrderId { get; set; }
        /// <summary>
        /// 服务单Sapid
        /// </summary>
        public int? ServiceOrderSapId { get; set; }
        /// <summary>
        /// 需要删除附件id
        /// </summary>
        public List<string> DelFileIds { get; set; }

        public List<AddOrUpdateoutsourcexpensesReq> OutsourcExpenses { get; set; }
        //todo:添加自己的请求字段
    }
}