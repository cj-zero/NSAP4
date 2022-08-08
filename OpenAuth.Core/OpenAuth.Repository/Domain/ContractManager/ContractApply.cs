﻿//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a CodeSmith Template.
//
//     DO NOT MODIFY contents of this file. Changes to this
//     file will be lost if the code is regenerated.
//     Author:RenChun Xia
// </autogenerated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using OpenAuth.Repository.Core;
using Infrastructure.AutoMapper;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 合同申请表
    /// </summary>
    [Table("contractapply")]
    public partial class ContractApply : Entity
    {
        public ContractApply()
        {
            this.ContractNo = string.Empty;
            this.CustomerCode = string.Empty;
            this.CustomerName = string.Empty;
            this.CompanyType = string.Empty;
            this.ContractType = string.Empty;
            this.QuotationNo = string.Empty;
            this.SaleNo = string.Empty;
            this.FlowInstanceId = string.Empty;
            this.Remark = string.Empty;
            this.CreateId = string.Empty;
            this.CreateTime = DateTime.Now;
        }

        /// <summary>
        /// 合同申请单编号
        /// </summary>
        [Description("合同申请单编号")]
        public string ContractNo { get; set; }

        /// <summary>
        /// 客户代码
        /// </summary>
        [Description("客户代码")]
        public string CustomerCode { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        [Description("客户名称")]
        public string CustomerName { get; set; }

        /// <summary>
        /// 所属公司
        /// </summary>
        [Description("所属公司")]
        public string CompanyType { get; set; }

        /// <summary>
        /// 合同类型
        /// </summary>
        [Description("合同类型")]
        public string ContractType { get; set; }

        /// <summary>
        /// 报价单号
        /// </summary>
        [Description("报价单号")]
        public string QuotationNo { get; set; }

        /// <summary>
        /// 销售单号
        /// </summary>
        [Description("销售单号")]
        public string SaleNo { get; set; }

        /// <summary>
        /// 是否为草稿
        /// </summary>
        [Description("是否为草稿")]
        public bool IsDraft { get; set; }

        /// <summary>
        /// 是否上传原件
        /// </summary>
        [Description("是否上传原件")]
        public bool IsUploadOriginal { get; set; }

        /// <summary>
        /// 工作流程Id
        /// </summary>
        [Description("工作流程Id")]
        [Browsable(false)]
        public string FlowInstanceId { get; set; }

        /// <summary>
        /// 合同下载次数
        /// </summary>
        [Description("合同下载次数")]
        public int DownloadNumber { get; set; }

        /// <summary>
        /// 合同状态
        /// </summary>
        [Description("合同状态")]
        public string ContractStatus { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [Description("备注")]
        public string Remark { get; set; }

        /// <summary>
        /// 申请人
        /// </summary>
        [Description("申请人")]
        public string CreateId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public System.DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Description("更新时间")]
        public System.DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 合同文件类型集合
        /// </summary>
        public virtual List<ContractFileType> ContractFileTypeList { get; set; }

        /// <summary>
        /// 合同申请单操作历史
        /// </summary>
        public virtual List<ContractOperationHistory> contractOperationHistoryList { get; set; }
    }
}