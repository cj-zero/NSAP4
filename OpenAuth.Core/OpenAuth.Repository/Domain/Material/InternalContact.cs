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
using OpenAuth.Repository.Domain.Material;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
	/// 
	/// </summary>
    [Table("internalcontact")]
    public partial class InternalContact : BaseEntity<int>
    {
        public InternalContact()
        {
          this.Theme= string.Empty;
          this.CardCode= string.Empty;
          this.CardName= string.Empty;
          this.Status= 0;
          this.RdmsNo= string.Empty;
          this.SaleOrderNo= string.Empty;
          this.ProductionNo= string.Empty;
          this.AdaptiveModel= string.Empty;
          this.AdaptiveRange= string.Empty;
          this.Reason= string.Empty;
          this.CheckApproveId= string.Empty;
          this.CheckApprove= string.Empty;
          this.DevelopApproveId= string.Empty;
          this.DevelopApprove= string.Empty;
          this.Content= string.Empty;
          this.CreateTime= DateTime.Now;
          this.CreateUserId= string.Empty;
          this.CreateUser= string.Empty;
            this.IsTentative = false;
            this.MaterialOrder = string.Empty;
        }

        /// <summary>
        /// IW号
        /// </summary>
        public string IW { get; set; }

        //public bool IsStop { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Theme { get; set; }
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
        public int?  Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string RdmsNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SaleOrderNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ProductionNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AdaptiveModel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AdaptiveRange { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Reason { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string CheckApproveId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CheckApprove { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string DevelopApproveId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DevelopApprove { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Content { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? ApproveTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? ExecTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CreateTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string CreateUserId { get; set; }
        /// <summary>
        /// 流程ID
        /// </summary>
        public string FlowInstanceId { get; set; }
        /// <summary>
        /// 是否暂定
        /// </summary>
        public bool IsTentative { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CreateUser { get; set; }
        /// <summary>
        /// 物料生成订单
        /// </summary>
        public string MaterialOrder { get; set; }

        public List<InternalContactAttchment> InternalContactAttchments { get; set; }
        public List<InternalContactBatchNumber> InternalContactBatchNumbers { get; set; }
        public List<InternalContactDeptInfo> InternalContactDeptInfos { get; set; }
        public List<InternalcontactMaterial> InternalcontactMaterials { get; set; }
        public List<InternalContactTask> InternalContactTasks { get; set; }
        public List<InternalContactServiceOrder> InternalContactServiceOrders { get; set; }
        public List<InternalContactProduction> InternalContactProductions { get; set; }
        public override void GenerateDefaultKeyVal()
        {

        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}