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
using OpenAuth.Repository.Domain;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
	/// 工单表
	/// </summary>
    [Table("serviceworkorder")]
    public partial class ServiceWorkOrder
    {
        public ServiceWorkOrder()
        {
            this.ServiceOrderId = 0;
            this.SubmitDate = DateTime.Now;
            this.SubmitUserId = string.Empty;
            this.Remark = string.Empty;
            this.FromTheme = string.Empty;
            this.ProblemTypeId = string.Empty;
            this.MaterialCode = string.Empty;
            this.MaterialDescription = string.Empty;
            this.ManufacturerSerialNumber = string.Empty;
            this.InternalSerialNumber = string.Empty;
            this.WarrantyEndDate = DateTime.Now;
            this.CreateTime = DateTime.Now;
            this.BookingDate = DateTime.Now;
            this.VisitTime = DateTime.Now;
            this.LiquidationDate = DateTime.Now;
        }
        /// <summary>
        /// 工单ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 服务单Id
        /// </summary>
        [Description("服务单Id")]
        //[Browsable(false)]
        public int ServiceOrderId { get; set; }
        /// <summary>
        /// 服务单
        /// </summary>
        public virtual ServiceOrder ServiceOrder { get; set; }
        /// <summary>
        /// 优先级 4-紧急 3-高 2-中 1-低
        /// </summary>
        [Description("优先级 4-紧急 3-高 2-中 1-低")]
        public int? Priority { get; set; }
        /// <summary>
        /// 服务类型 1-免费 2-收费
        /// </summary>
        [Description("服务类型 1-免费 2-收费")]
        public int? FeeType { get; set; }
        /// <summary>
        /// 工单提交时间
        /// </summary>
        [Description("工单提交时间")]
        public System.DateTime? SubmitDate { get; set; }
        /// <summary>
        /// 工单提交用户Id
        /// </summary>
        [Description("工单提交用户Id")]
        [Browsable(false)]
        public string SubmitUserId { get; set; }
        /// <summary>
        /// APP用户Id
        /// </summary>
        [Description("APP用户Id")]
        //[Browsable(false)]
        public int? AppUserId { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Description("备注")]
        public string Remark { get; set; }
        /// <summary>
        /// 呼叫状态  1-待处理 2-已排配 3-已外出 4-已挂起 5-已接收 6-已解决 7-已回访
        /// </summary>
        [Description("呼叫状态")]
        public int? Status { get; set; }
        /// <summary>
        /// App当前流程处理用户Id
        /// </summary>
        [Description("App当前流程处理用户Id")]
        //[Browsable(false)]
        public int? CurrentUserId { get; set; }
        /// <summary>
        /// 呼叫主题
        /// </summary>
        [Description("呼叫主题")]
        public string FromTheme { get; set; }
        /// <summary>
        /// 问题类型Id
        /// </summary>
        [Description("问题类型Id")]
        //[Browsable(false)]
        public string ProblemTypeId { get; set; }
        /// <summary>
        /// 问题类型
        /// </summary>
        [Description("问题类型")]
        public virtual ProblemType ProblemType { get; set; }
        /// <summary>
        /// 呼叫类型1-提交呼叫 2-在线解答（已解决）
        /// </summary>
        [Description("呼叫类型")]
        public int? FromType { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        [Description("物料编码")]
        public string MaterialCode { get; set; }
        /// <summary>
        /// 物料描述
        /// </summary>
        [Description("物料描述")]
        public string MaterialDescription { get; set; }
        /// <summary>
        /// 制造商序列号
        /// </summary>
        [Description("制造商序列号")]
        public string ManufacturerSerialNumber { get; set; }
        /// <summary>
        /// 内部序列号
        /// </summary>
        [Description("内部序列号")]
        public string InternalSerialNumber { get; set; }
        /// <summary>
        /// 保修结束日期
        /// </summary>
        [Description("保修结束日期")]
        public System.DateTime? WarrantyEndDate { get; set; }
        /// <summary>
        /// 创建日期
        /// </summary>
        [Description("创建日期")]
        public System.DateTime? CreateTime { get; set; }
        /// <summary>
        /// 预约日期
        /// </summary>
        [Description("预约日期")]
        public System.DateTime? BookingDate { get; set; }
        /// <summary>
        /// 上门时间
        /// </summary>
        [Description("上门时间")]
        public System.DateTime? VisitTime { get; set; }
        /// <summary>
        /// 清算日期
        /// </summary>
        [Description("清算日期")]
        public System.DateTime? LiquidationDate { get; set; }
        /// <summary>
        /// 解决方案Id
        /// </summary>
        [Description("解决方案Id")]
        [Browsable(false)]
        public string SolutionId { get; set; }
        public virtual Solution Solution { get; set; }
        /// <summary>
        /// 完工报告Id
        /// </summary>
        [Description("完工报告Id")]
        [Browsable(false)]
        public string CompletionReportId { get; set; }
        /// <summary>
        /// 完工报告
        /// </summary>
        //[Description("完工报告")]
        //public virtual CompletionReport CompletionReport { get; set; }
        /// <summary>
        /// 服务合同
        /// </summary>
        [Description("服务合同")]
        public string ContractId { get; set; }
        /// <summary>
        /// 故障描述
        /// </summary>
        [Description("故障描述")]
        public string TroubleDescription { get; set; }
        /// <summary>
        /// 过程描述
        /// </summary>
        [Description("过程描述")]
        public string ProcessDescription { get; set; }

        /// <summary>
        /// 接单类型 0未接单 1电话服务 2上门服务 3电话服务(已拨打)
        /// </summary>
        [Description("接单类型")]
        public int OrderTakeType { get; set; }

        /// <summary>
        ///核对设备 0未核对 1正确 2失败
        /// </summary>
        [Description("接单类型")]
        public int IsCheck { get; set; }

        /// <summary>
        /// 技术员留言消息
        /// </summary>
        [Description("技术员留言消息")]
        public virtual List<ServiceOrderMessage> ServiceOrderMessages { get; set; }




    }
}