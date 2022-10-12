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
    [Table("blamebelong")]
    public partial class BlameBelong : BaseEntity<int>
    {
        public BlameBelong()
        {
            this.VestinOrg = string.Empty;
            this.Description = string.Empty;
            this.ProductionOrg = string.Empty;
            this.CardCode = string.Empty;
            this.CardName = string.Empty;
            this.FlowInstanceId = string.Empty;
            this.CreateUserId = string.Empty;
            this.CreateUser = string.Empty;
            this.CreateTime = DateTime.Now;
            this.SerialNumber = string.Empty;
            this.CardCode = string.Empty;
            this.CardName = string.Empty;
            this.ProductionOrg = string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string VestinOrg { get; set; }
        /// <summary>
        /// 责任单据类型 1-服务单 2-生产单 3-采购单 4-其他
        /// </summary>
        [Description("")]
        public int? DocType { get; set; }
        /// <summary>
        /// 责任依据 1-服务质量 2-生产质量 3-研发质量 4-采购质量 5-工程设计质量 6-需求不请 7-需求变更
        /// </summary>
        [Description("")]
        public string Basis { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? OrderNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? AffectMoney { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? SaleOrderId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ProductionOrg { get; set; }
        /// <summary>
        /// 1-撤回 2-责任部门审核 3-人事审核 4-责任金额判断 5-出纳 6-结束
        /// </summary>
        [Description("")]
        public int? Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CardCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CardName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string FlowInstanceId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string CreateUserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CreateUser { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CreateTime { get; set; }
        /// <summary>
        /// 序列号
        /// </summary>
        public string SerialNumber { get; set; }
        /// <summary>
        /// 来源 1-erp 2-app
        /// </summary>
        public int? Source { get; set; }
        public virtual List<BlameBelongOrg> BlameBelongOrgs { get; set; }
        public virtual List<BlameBelongFile> BlameBelongFiles { get; set; }

        public override void GenerateDefaultKeyVal()
        {

        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}