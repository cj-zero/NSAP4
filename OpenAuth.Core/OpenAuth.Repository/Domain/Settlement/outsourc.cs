//------------------------------------------------------------------------------
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
    [Table("Outsourc")]
    public partial class Outsourc : BaseEntity<int>
    {
        public Outsourc()
        {
          this.PayTime= DateTime.Now;
          this.Remark= string.Empty;
          this.CreateUser= string.Empty;
          this.CreateUserId= string.Empty;
          this.CreateTime= DateTime.Now;
          this.UpdateTime= DateTime.Now;
        }

        
        /// <summary>
        /// 支付时间
        /// </summary>
        [Description("支付时间")]
        public System.DateTime? PayTime { get; set; }
        /// <summary>
        /// 总金额
        /// </summary>
        [Description("总金额")]
        public decimal? TotalMoney { get; set; }
        /// <summary>
        /// 服务方式 1-上门 2-远程
        /// </summary>
        [Description("结算方式")]
        public int? ServiceMode { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Description("备注")]
        public string Remark { get; set; }
        /// <summary>
        /// 创建人姓名
        /// </summary>
        [Description("创建人姓名")]
        public string CreateUser { get; set; }
        /// <summary>
        /// 创建人id
        /// </summary>
        [Description("创建人id")]
        [Browsable(false)]
        public string CreateUserId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public System.DateTime? CreateTime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        [Description("修改时间")]
        public System.DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 审批流程id
        /// </summary>
        [Description("审批流程id")]
        public string FlowInstanceId { get; set; }

        /// <summary>
        /// 是否存在部分驳回
        /// </summary>
        [Description("是否存在部分驳回")]
        public bool IsRejected { get; set; }
        /// <summary>
        /// 报价单id
        /// </summary>
        [Description("报价单id")]
        public int? QuotationId { get; set; }

        /// <summary>
        /// 报表ID
        /// </summary>
        public int? OutsourcReportId { get; set; }

        /// <summary>
        /// 费用明细表
        /// </summary>
        public List<OutsourcExpenses> OutsourcExpenses { get; set; }

        public override void GenerateDefaultKeyVal()
        {

        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}