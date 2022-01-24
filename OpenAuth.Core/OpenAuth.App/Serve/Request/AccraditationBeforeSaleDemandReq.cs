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

namespace OpenAuth.App.Request
{
    /// <summary>
    /// 售前流程审批
    /// </summary>
    public partial class AccraditationBeforeSaleDemandReq
    {
        /// <summary>
        /// 售前申请流程Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 是否驳回
        /// </summary>
        public bool IsReject { get; set; }
        /// <summary>
        /// 确定需求人员Id
        /// </summary>
        public string FactDemandUserId { get; set; }
        /// <summary>
        /// 确定需求人员
        /// </summary>
        public string FactDemandUser { get; set; }
        /// <summary>
        /// 审批流程Id
        /// </summary>
        public string FlowInstanceId { get; set; }
        /// <summary>
        /// 当前流程状态0-草稿 1-销售提交需求 2-销售总助审批 3-需求组提交需求 4-研发总助审批 5-研发确认 6-总经理审批 7-立项 8-需求提交 9-研发提交10-测试提交11-实施提交12-客户验收(流程结束)13-驳回状态
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 需求简述~申请人简单描述需求内容
        /// </summary>
        public string DemandContents { get; set; }
        /// <summary>
        /// 需求初次沟通
        /// </summary>
        public string FirstConnects { get; set; }
        /// <summary>
        /// 预估开发成本
        /// </summary>
        public int? PredictDevCost { get; set; }
        /// <summary>
        /// 开发预估工期
        /// </summary>
        public int? DevEstimate { get; set; }
        /// <summary>
        /// 测试预估工期
        /// </summary>
        public int? TestEstimate { get; set; }
        /// <summary>
        /// 审批流程时的备注信息
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 是否需要开发部署项目：默认0
        /// </summary>
        public int? IsDevDeploy { get; set; }
        /// <summary>
        /// 实施负责人Id
        /// </summary>
        public string ExecutorUserId { get; set; }
        /// <summary>
        /// 实施负责人
        /// </summary>
        public string ExecutorName { get; set; }
        /// <summary>
        /// 是否收费 默认空
        /// 1:“单独研发收费”，2“免费技术支持”
        /// </summary>
        public int? IsCharge { get; set; }
        /// <summary>
        /// 开发投入预估（开发预估工期+测试预估工期）*预估开发成本
        /// </summary>
        public int? DevCost { get; set; }        
        /// <summary>
        /// 是否关联项目：默认0不关联项目
        /// </summary>
        public int? IsRelevanceProject { get; set; }
        /// <summary>
        /// 审批流程时上传的附件信息
        /// </summary>
        public List<string> Attchments { get; set; }

        /// <summary>
        /// 研发部门id
        /// </summary>
        public List<string> DevDeptInfoIds { get; set; }
        /// <summary>
        /// 确定开发部门列表
        /// </summary>
        public List<BeforeSaleDemandDeptInfo> BeforeSaleDemandDeptInfos { get; set; }

        /// <summary>
        /// 关联项目名称
        /// </summary>
        public string BeforeSaleDemandProjectName { get; set; }
        /// <summary>
        /// 流程关联项目信息
        /// </summary>
        public BeforeSaleDemandProject BeforeSaleDemandProject { get; set; }

        /// <summary>
        /// 项目链接
        /// </summary>
        public string ProjectUrl { get; set; }
        /// <summary>
        /// 需求文档/URL
        /// </summary>
        public string ProjectDocURL { get; set; }
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
        /// 实际研发开始日期
        /// </summary>
       public DateTime? ActualDevStartDate { get; set; }
        /// <summary>
        /// 实际研发结束日期
        /// </summary>
        public DateTime? ActualDevEndDate { get; set; }
        /// <summary>
        /// 实际测试开始日期
        /// </summary>
        public DateTime? ActualTestStartDate { get; set; }
        /// <summary>
        /// 实际测试结束日期
        /// </summary>
        public DateTime? ActualTestEndDate { get; set; }
        /// <summary>
        /// 项目排期列表
        /// </summary>
        public List<BeforeSaleProScheduling> BeforeSaleProSchedulings { get; set; }

    }
}