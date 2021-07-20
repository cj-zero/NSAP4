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

namespace OpenAuth.Repository.Domain.Settlement
{
    /// <summary>
	/// 
	/// </summary>
    [Table("OutsourcExpenses")]
    public partial class OutsourcExpenses : Entity
    {
        public OutsourcExpenses()
        {
            this.From = string.Empty;
            this.To = string.Empty;
            this.FromLng = string.Empty;
            this.FromLat = string.Empty;
            this.ToLng = string.Empty;
            this.ToLat = string.Empty;
            this.IsOverseas = false;
        }


        /// <summary>
        /// 结算表id
        /// </summary>
        [Description("结算表id")]
        public int? OutsourcId { get; set; }
        /// <summary>
        /// 费用类型
        /// </summary>
        [Description("费用类型")]
        public int? ExpenseType { get; set; }
        /// <summary>
        /// 服务单id
        /// </summary>
        [Description("服务单id")]
        [Browsable(false)]
        public int? ServiceOrderId { get; set; }
        /// <summary>
        /// 服务单sapid
        /// </summary>
        [Description("服务单sapid")]
        [Browsable(false)]
        public int? ServiceOrderSapId { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        [Description("金额")]
        public decimal? Money { get; set; }
        /// <summary>
        /// 出发地
        /// </summary>
        [Description("出发地")]
        public string From { get; set; }
        /// <summary>
        /// 到达地
        /// </summary>
        [Description("到达地")]
        public string To { get; set; }
        /// <summary>
        /// 出发地经度
        /// </summary>
        [Description("出发地经度")]
        public string FromLng { get; set; }
        /// <summary>
        /// 出发地经度
        /// </summary>
        [Description("出发地经度")]
        public string FromLat { get; set; }
        /// <summary>
        /// 到达地经度
        /// </summary>
        [Description("到达地经度")]
        public string ToLng { get; set; }
        /// <summary>
        /// 到达地纬度
        /// </summary>
        [Description("到达地纬度")]
        public string ToLat { get; set; }
        /// <summary>
        /// 天数
        /// </summary>
        [Description("天数")]
        public int? Days { get; set; }
        /// <summary>
        /// 工时
        /// </summary>
        [Description("工时")]
        public int? ManHour { get; set; }

        /// <summary>
        /// 完工时间
        /// </summary>
        [Description("完工时间")]
        public DateTime? CompleteTime { get; set; }

        /// <summary>
        /// 编号
        /// </summary>
        [Description("编号")]
        public int? SerialNumber { get; set; }

        /// <summary>
        /// 终端客户代码
        /// </summary>
        [Description("终端客户代码")]
        public string TerminalCustomerId { get; set; }

        /// <summary>
        /// 终端客户名称
        /// </summary>
        [Description("终端客户名称")]
        public string TerminalCustomer { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        [Description("开始时间")]
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        [Description("结束时间")]
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 是否海外
        /// </summary>
        [Description("是否海外")]
        public bool IsOverseas { get; set; }

        /// <summary>
        /// 附件表
        /// </summary>
        public List<OutsourcExpensesPicture> outsourcexpensespictures { get; set; }
    }
}