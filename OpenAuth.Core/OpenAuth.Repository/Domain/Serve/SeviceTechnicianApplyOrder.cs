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
	/// 
	/// </summary>
    [Table("sevicetechnicianapplyorder")]
    public partial class SeviceTechnicianApplyOrder : Entity
    {
        public SeviceTechnicianApplyOrder()
        {
            this.MaterialType = string.Empty;
            this.ManufSN = string.Empty;
            this.ItemCode = string.Empty;
            this.OrginalManufSN = string.Empty;
            this.CreateTime = DateTime.Now;
            this.FromTheme = string.Empty;
            this.ProblemTypeId = string.Empty;
        }


        /// <summary>
        /// 设备类型
        /// </summary>
        [Description("设备类型")]
        public string MaterialType { get; set; }

        /// <summary>
        /// 制造商序列号
        /// </summary>
        [Description("制造商序列号")]
        public string ManufSN { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        [Description("物料编码")]
        public string ItemCode { get; set; }

        /// <summary>
        /// 服务单Id
        /// </summary>
        [Description("服务单Id")]
        [Browsable(false)]
        public int? ServiceOrderId { get; set; }

        /// <summary>
        ///原始序列号 
        /// </summary>
        [Description("原始序列号")]
        public string OrginalManufSN { get; set; }

        /// <summary>
        /// 工单状态
        /// </summary>
        [Description("工单状态")]
        public int? Status { get; set; }

        /// <summary>
        /// 接单类型
        /// </summary>
        [Description("接单类型")]
        public int? OrderTakeType { get; set; }

        /// <summary>
        /// 技术员Id
        /// </summary>
        [Description("技术员Id")]
        [Browsable(false)]
        public int? TechnicianId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public System.DateTime? CreateTime { get; set; }

        /// <summary>
        /// 呼叫主题
        /// </summary>
        [Description("呼叫主题")]
        public string FromTheme { get; set; }

        /// <summary>
        /// 呼叫类型 1-提交呼叫 2-在线解答（已解决）
        /// </summary>
        [Description("呼叫类型")]
        public int? FromType { get; set; }

        /// <summary>
        /// 问题类型
        /// </summary>
        [Description("问题类型")]
        [Browsable(false)]
        public string ProblemTypeId { get; set; }

        /// <summary>
        /// 服务台是否处理 1已处理 0未处理
        /// </summary>
        [Description("是否处理")]
        public int? IsSolved { get; set; }

        /// <summary>
        /// 原始工单号
        /// </summary>
        [Description("原始工单号")]
        public int? OrginalWorkOrderId { get; set; }

        /// <summary>
        /// 处理结果 0不处理 1修改 2新增
        /// </summary>
        [Description("处理结果")]
        public int? SolvedResult { get; set; }

        /// <summary>
        /// 处理时间
        /// </summary>
        [Description("处理时间")]
        public System.DateTime? SolvedTime { get; set; }

        /// <summary>
        /// 技术员NSAPId
        /// </summary>
        [Description("技术员NSAPId")]
        public string CurrentUserNsapId { get; set; }

        /// <summary>
        /// 当前接单技术员名称
        /// </summary>
        [Description("当前接单技术员名称")]
        public string CurrentUser { get; set; }

        /// <summary>
        /// 内部序列号
        /// </summary>
        [Description("内部序列号")]
        public string InternalSerialNumber { get; set; }

        /// <summary>
        /// 物料描述
        /// </summary>
        [Description("物料描述")]
        public string MaterialDescription { get; set; }

        /// <summary>
        /// 问题类型名称
        /// </summary>
        [Description("问题类型名称")]
        public string ProblemTypeName { get; set; }

        /// <summary>
        /// 服务合同
        /// </summary>
        [Description("服务合同")]
        public string ContractId { get; set; }

        /// <summary>
        /// 保修结束日期
        /// </summary>
        [Description("保修结束日期")]
        public System.DateTime? WarrantyEndDate { get; set; }

    }
}