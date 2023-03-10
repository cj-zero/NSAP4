using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using OpenAuth.Repository.Core;
using AutoMapper.Configuration.Annotations;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
	/// 
	/// </summary>
    [Table("servicedailyexpends")]
    public partial class ServiceDailyExpends : Entity
    {
        public ServiceDailyExpends()
        {
            this.FeeType = string.Empty;
            this.TrafficType = string.Empty;
            this.Transport = string.Empty;
            this.From = string.Empty;
            this.To = string.Empty;
            this.InvoiceNumber = string.Empty;
            this.Remark = string.Empty;
            this.CreateTime = DateTime.Now;
            this.ExpenseCategory = string.Empty;
            this.CreateUserId = string.Empty;
            this.CreateUserName = string.Empty;
            this.InvoiceTime = DateTime.Now;
            this.SellerName = string.Empty;
        }

        /// <summary>
        /// 费用类型
        /// </summary>
        [Description("服务单Id")]
        public int ServiceOrderId { get; set; }

        /// <summary>
        /// 费用类型
        /// </summary>
        [Description("费用类型")]
        public string FeeType { get; set; }
        /// <summary>
        /// 报销单据类型(1 差补费， 2 交通费， 3住宿费， 4 其他费)
        /// </summary>
        [Description("类型(1 差补费， 2 交通费， 3住宿费， 4 其他费)")]
        public int DailyExpenseType { get; set; }
        /// <summary>
        /// 序号
        /// </summary>
        [Description("序号")]
        public int? SerialNumber { get; set; }
        /// <summary>
        /// 交通类型
        /// </summary>
        [Description("交通类型")]
        public string TrafficType { get; set; }
        /// <summary>
        /// 交通工具
        /// </summary>
        [Description("交通工具")]
        public string Transport { get; set; }
        /// <summary>
        /// 出发地
        /// </summary>
        [Description("出发地")]
        public string From { get; set; }
        /// <summary>
        /// 目的地
        /// </summary>
        [Description("目的地")]
        public string To { get; set; }

        /// <summary>
        /// 出发地址经度
        /// </summary>
        [Description("出发地址经度")]
        public string FromLng { get; set; }

        /// <summary>
        /// 出发地址纬度
        /// </summary>
        [Description("出发地址纬度")]
        public string FromLat { get; set; }

        /// <summary>
        /// 到达地址经度
        /// </summary>
        [Description("到达地址经度")]
        public string ToLng { get; set; }

        /// <summary>
        /// 到达地址纬度
        /// </summary>
        [Description("到达地址纬度")]
        public string ToLat { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        [Description("金额")]
        public decimal? Money { get; set; }
        /// <summary>
        /// 发票号码
        /// </summary>
        [Description("发票号码")]
        public string InvoiceNumber { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Description("备注")]
        public string Remark { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public System.DateTime? CreateTime { get; set; }
        /// <summary>
        /// 天数
        /// </summary>
        [Description("天数")]
        public int? Days { get; set; }
        /// <summary>
        /// 总金额
        /// </summary>
        [Description("总金额")]
        public decimal? TotalMoney { get; set; }
        /// <summary>
        /// 费用类别
        /// </summary>
        [Description("费用类别")]
        public string ExpenseCategory { get; set; }
        /// <summary>
        /// 创建用户Id
        /// </summary>
        [Description("创建用户Id")]
        [Browsable(false)]
        public string CreateUserId { get; set; }
        /// <summary>
        /// 创建用户
        /// </summary>
        [Description("创建用户")]
        public string CreateUserName { get; set; }
        /// <summary>
        /// 开票日期
        /// </summary>
        [Description("开票日期")]
        public System.DateTime? InvoiceTime { get; set; }
        /// <summary>
        /// 开票单位
        /// </summary>
        [Description("开票单位")]
        public string SellerName { get; set; }

        /// <summary>
        /// 报销附件集合
        /// </summary>
        [Description("报销附件集合")]
        public string ReimburseAttachment { get; set; }
    }
}