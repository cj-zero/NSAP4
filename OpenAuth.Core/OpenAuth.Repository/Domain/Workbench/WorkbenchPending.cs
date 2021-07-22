using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Workbench
{
    /// <summary>
	/// 个人工作台待处理
	/// </summary>
    [Table("WorkbenchPending")]
    public class WorkbenchPending : Entity
    {
        /// <summary>
        /// 审批序号
        /// </summary>
        [Description("审批序号")]
        [Key]
        public int ApprovalNumber { get; set; }
        /// <summary>
        /// 服务单号
        /// </summary>
        [Description("服务单号")]
        public int ServiceOrderId { get; set; }
        /// <summary>
        /// 服务单Sap号
        /// </summary>
        [Description("服务单Sap号")]
        public int ServiceOrderSapId { get; set; }
        
        /// <summary>
        /// 总金额
        /// </summary>
        [Description("总金额")]
        public decimal? TotalMoney { get; set; }
        /// <summary>
        /// 订单类型 1销售订单 2 退料单 3 个人结算单 4 报销单
        /// </summary> 
        [Description("订单类型")]
        public int OrderType { get; set; }
        /// <summary>
        /// 原单号
        /// </summary> 
        [Description("原单号")]
        public int SourceNumbers { get; set; }
        
        /// <summary>
        /// 备注
        /// </summary>
        [Description("备注")]
        public string Remark { get; set; }
        /// <summary>
        /// 申请人
        /// </summary>
        [Description("申请人")]
        public string Petitioner { get; set; }

        /// <summary>
        /// 申请人Id
        /// </summary>
        [Description("申请人Id")]
        public string PetitionerId { get; set; }
        
        /// <summary>
        /// 终端客户名称
        /// </summary>
        [Description("终端客户名称")]
        public string TerminalCustomer { get; set; }
        /// <summary>
        /// 终端客户代码
        /// </summary>
        [Description("终端客户代码")]
        public string TerminalCustomerId { get; set; }
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


    }
}
