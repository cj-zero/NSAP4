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
using AutoMapper.Configuration.Annotations;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
	/// 售前需求对接申请表
	/// </summary>
    [Table("beforesaledemand")]
    public partial class BeforeSaleDemand : BaseEntity<int>
    {
        public BeforeSaleDemand()
        {
          this.BeforeSaleDemandProjectId = 0;
          this.BeforeSaleDemandProjectName = string.Empty;
          this.BeforeDemandCode= string.Empty;
          this.ApplyUserId= string.Empty;
          this.ApplyUserName= string.Empty;
          this.ApplyDate= DateTime.Now;
          this.ExpectUserId= string.Empty;
          this.ExpectDate= DateTime.Now;
          this.ExpectUserName= string.Empty;
          this.FactDemandUserId= string.Empty;
          this.FactDemandUser= string.Empty;
          this.CustomerId= string.Empty;
          this.CustomerName= string.Empty;
          this.CustomerLinkMan= string.Empty;
          this.LinkManPhone= string.Empty;
          this.FlowInstanceId= string.Empty;
          this.Status= 0;
          this.DemandContents= string.Empty;
          this.FirstConnects= string.Empty;
          this.Remark= string.Empty;
          this.CreateUserName= string.Empty;
          this.CreateUserId= string.Empty;
          this.CreateTime= DateTime.Now;
          this.UpdateTime= DateTime.Now;
        }
        /// <summary>
        /// 关联售前项目Id
        /// </summary>
        [Description("关联售前项目Id")]
        public int? BeforeSaleDemandProjectId { get; set; }
        /// <summary>
        /// 关联售前项目名称
        /// </summary>
        [Description("关联项目名称")]
        public string BeforeSaleDemandProjectName { get; set; }
        /// <summary>
        /// 售前需求对接申请编号
        /// </summary>
        [Description("售前需求对接申请编号")]
        public string BeforeDemandCode { get; set; }
        /// <summary>
        /// 申请人Id
        /// </summary>
        [Description("申请人Id")]
        [Browsable(false)]
        public string ApplyUserId { get; set; }
        /// <summary>
        /// 申请人名称
        /// </summary>
        [Description("申请人名称")]
        public string ApplyUserName { get; set; }
        /// <summary>
        /// 申请日期
        /// </summary>
        [Description("申请日期")]
        public System.DateTime? ApplyDate { get; set; }
        /// <summary>
        /// 期望需求人员Id
        /// </summary>
        [Description("期望需求人员Id")]
        [Browsable(false)]
        public string ExpectUserId { get; set; }
        /// <summary>
        /// 希望协助日期
        /// </summary>
        [Description("希望协助日期")]
        public System.DateTime? ExpectDate { get; set; }
        /// <summary>
        /// 期望的需求人员
        /// </summary>
        [Description("期望的需求人员")]
        public string ExpectUserName { get; set; }
        /// <summary>
        /// 确定需求人员Id
        /// </summary>
        [Description("确定需求人员Id")]
        [Browsable(false)]
        public string FactDemandUserId { get; set; }
        /// <summary>
        /// 确定需求人员
        /// </summary>
        [Description("确定需求人员")]
        public string FactDemandUser { get; set; }
        /// <summary>
        /// 客户编号
        /// </summary>
        [Description("客户编号")]
        [Browsable(false)]
        public string CustomerId { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        [Description("")]
        public string CustomerName { get; set; }
        /// <summary>
        /// 客户联系人
        /// </summary>
        [Description("客户联系人")]
        public string CustomerLinkMan { get; set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        [Description("联系电话")]
        public string LinkManPhone { get; set; }
        /// <summary>
        /// 审批流程Id
        /// </summary>
        [Description("审批流程Id")]
        [Browsable(false)]
        public string FlowInstanceId { get; set; }
        /// <summary>
        /// 状态0-草稿 1-销售提交需求 2-销售总助审批 3-需求组提交需求 4-研发总助审批 5-研发确认 6-总经理审批 7-立项 8-需求提交 9-研发提交10-测试提交11-实施提交12-客户验收(流程结束)
        /// </summary>
        [Description("状态0-草稿 1-销售提交需求 2-销售总助审批 3-需求组提交需求 4-研发总助审批 5-研发确认 6-总经理审批 7-立项 8-需求提交 9-研发提交10-测试提交11-实施提交12-客户验收")]
        public int Status { get; set; }

        /// <summary>
        /// 是否是草稿
        /// </summary>
        public bool IsDraft { get; set; }
        /// <summary>
        /// 需求简述~申请人简单描述需求内容
        /// </summary>
        [Description("需求简述~申请人简单描述需求内容")]
        public string DemandContents { get; set; }
        /// <summary>
        /// 需求初次沟通
        /// </summary>
        [Description("需求初次沟通")]
        public string FirstConnects { get; set; }
        /// <summary>
        /// 预估开发成本
        /// </summary>
        [Description("预估开发成本")]
        public int? PredictDevCost { get; set; }
        /// <summary>
        /// 开发预估工期
        /// </summary>
        [Description("开发预估工期")]
        public int? DevEstimate { get; set; }
        /// <summary>
        /// 测试预估工期
        /// </summary>
        [Description("测试预估工期")]
        public int? TestEstimate { get; set; }
        /// <summary>
        /// 备注说明
        /// </summary>
        [Description("备注说明")]
        public string Remark { get; set; }
        /// <summary>
        /// 是否需要开发实施
        /// </summary>
        [Description("是否需要开发实施")]
        public int? IsDevDeploy { get; set; }
        /// <summary>
        /// 是否关联项目
        /// </summary>
        [Description("是否关联项目")]
        public int? IsRelevanceProject { get; set; }

        /// <summary>
        /// 开发投入预估（开发预估工期+测试预估工期）*预估开发成本
        /// </summary>
        public int? DevCost { get; set; }
        /// <summary>
        /// 是否收费 默认空
        /// 1:“单独研发收费”，2“免费技术支持”
        /// </summary>
        public int? IsCharge { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        [Description("创建人")]
        public string CreateUserName { get; set; }
        /// <summary>
        /// 创建人ID
        /// </summary>
        [Description("创建人ID")]
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
        /// 实施负责人Id
        /// </summary>
        [Description("实施负责人Id")]
        [Browsable(false)]
        public string ExecutorUserId { get; set; }
        /// <summary>
        /// 实施负责人
        /// </summary>
        [Description("实施负责人")]
        public string ExecutorName { get; set; }
        /// <summary>
        /// 实际开始日期
        /// </summary>
        [Description("实际开始日期")]
        public System.DateTime? ActualStartDate { get; set; }
        /// <summary>
        /// 提交日期
        /// </summary>
        [Description("提交日期")]
        public System.DateTime? SubmitDate { get; set; }
        /// <summary>
        /// 状态 1-立项 2-需求 3-开发 4-测试5-实施 6-验收 7-结束
        /// </summary>
        [Description("项目状态 1-立项 2-需求 3-开发 4-测试5-实施 6-验收 7-结束")]
        public int ProjectStatus { get; set; }
        /// <summary>
        /// 项目链接
        /// </summary>
        [Description("项目链接")]
        public string ProjectUrl { get; set; }
        /// <summary>
        /// 需求文档/URL
        /// </summary>
        [Description("需求文档/URL")]
        public string ProjectDocURL { get; set; }
        /// <summary>
        /// 实际开发开始日期
        /// </summary>
        [Description("实际开发开始日期")]
        public System.DateTime? ActualDevStartDate { get; set; }
        /// <summary>
        /// 实际开发结束日期
        /// </summary>
        [Description("实际开发结束日期")]
        public System.DateTime? ActualDevEndDate { get; set; }

        /// <summary>
        /// 实际测试开始日期
        /// </summary>
        [Description("实际测试开始日期")]
        public System.DateTime? ActualTestStartDate { get; set; }
        /// <summary>
        /// 实际测试结束日期
        /// </summary>
        [Description("实际测试结束日期")]
        public System.DateTime? ActualTestEndDate { get; set; }
        /// <summary>
        /// 售前申请附件
        /// </summary>
        public virtual List<BeforeSaleFiles> Beforesalefiles { get; set; }
        /// <summary>
        /// 关联单据
        /// </summary>
        public virtual List<BeforeSaleDemandOrders> BeforeSaleDemandOrders{ get; set; }
        /// <summary>
        /// 售前申请流程关联项目
        /// </summary>
        public virtual List<BeforeSaleDemandProject> Beforesaledemandprojects { get; set; }
        /// <summary>
        /// 项目排期表
        /// </summary>
        public virtual List<BeforeSaleProScheduling>  BeforeSaleProSchedulings { get; set; }
        /// <summary>
        /// 售前申请项目操作记录
        /// </summary>
        public virtual List<BeforeSaleDemandOperationHistory> Beforesaledemandoperationhistories { get; set; }

        /// <summary>
        /// 确定开发部门列表
        /// </summary>
        public virtual List<BeforeSaleDemandDeptInfo> BeforeSaleDemandDeptInfos { get; set; }

        public override void GenerateDefaultKeyVal()
        {
        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}