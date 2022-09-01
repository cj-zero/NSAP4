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
using Infrastructure.AutoMapper;
using OpenAuth.App.Response;
using OpenAuth.Repository.Core;
using OpenAuth.Repository.Domain;

namespace OpenAuth.App.Reponse
{
    /// <summary>
	/// 售前需求申请流程
	/// </summary>
    [Table("beforesaledemand")]
    [AutoMapTo(typeof(BeforeSaleDemand))]
    public partial class BeforeSaleDemandResp
    {

        /// <summary>
        /// 售前申请流程Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 售前需求申请流程编号
        /// </summary>
        public string BeforeDemandCode { get; set; }
        /// <summary>
        /// 申请人Id
        /// </summary>
        public string ApplyUserId { get; set; }
        /// <summary>
        /// 申请人名字
        /// </summary>
        public string ApplyUserName { get; set; }
        /// <summary>
        /// 申请日期
        /// </summary>
        public System.DateTime? ApplyDate { get; set; }
        /// <summary>
        /// 期望需求人员Id
        /// </summary>
        public string ExpectUserId { get; set; }
        /// <summary>
        /// 希望协助日期
        /// </summary>
        public System.DateTime? ExpectDate { get; set; }
        /// <summary>
        /// 期望的需求人员
        /// </summary>
        public string ExpectUserName { get; set; }
        /// <summary>
        /// 确定需求人员Id
        /// </summary>
        public string FactDemandUserId { get; set; }
        /// <summary>
        /// 确定需求人员
        /// </summary>
        public string FactDemandUser { get; set; }
        /// <summary>
        /// 客户编号
        /// </summary>
        public string CustomerId { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; }
        /// <summary>
        /// 客户联系人
        /// </summary>
        public string CustomerLinkMan { get; set; }
        /// <summary>
        /// 客户联系电话
        /// </summary>
        public string LinkManPhone { get; set; }
        /// <summary>
        /// 审批流程Id
        /// </summary>
        public string FlowInstanceId { get; set; }
        /// <summary>
        /// 状态 0-草稿 1-销售提交需求 2-销售总助审批 3-需求组提交需求 
        /// 4-研发总助审批 5-研发确认 6-总经理审批 7-立项 8-驳回
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 流程步骤 1-销售提交需求 2-销售总助审批 3-需求组提交需求 4-研发总助审批 5-研发确认 
        /// 6-总经理审批 7-立项 8-需求提交 9-研发提交 10-测试提交 11-实施提交 12-客户验收
        /// </summary>
        public int Step { get; set; }
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
        public string PredictDevCost { get; set; }
        /// <summary>
        /// 开发预估工期
        /// </summary>
        public int? DevEstimate { get; set; }
        /// <summary>
        /// 测试预估工期
        /// </summary>
        public int? TestEstimate { get; set; }
        /// <summary>
        /// 需求预估工期
        /// </summary>
        public int? DemandEstimate { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 是否是草稿 默认否false
        /// </summary>
        public bool IsDraft { get; set; }

        /// <summary>
        /// 开发投入预估（开发预估工期+测试预估工期）*预估开发成本
        /// </summary>
        public string DevCost { get; set; }
        /// <summary>
        /// 是否收费 默认空
        /// 1:“单独研发收费”，2“免费技术支持”
        /// </summary>
        public int? IsCharge { get; set; }
        /// <summary>
        /// 是否需要开发部署项目：默认0
        /// </summary>
        public int? IsDevDeploy { get; set; }
        /// <summary>
        /// 是否关联项目：默认0
        /// </summary>
        public int? IsRelevanceProject { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUserName { get; set; }
        /// <summary>
        /// 创建人Id
        /// </summary>
        public string CreateUserId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public System.DateTime? CreateTime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public System.DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 关联项目ID
        /// </summary>
        public int BeforeSaleDemandProjectId { get; set; }
        /// <summary>
        /// 关联项目名称
        /// </summary>
        public string BeforeSaleDemandProjectName { get; set; }

        //todo:添加自己的请求字段

        /// <summary>
        /// 售前需求申请流程附件Id集合
        /// </summary>
        public List<string> Attchments { get; set; }
        /// <summary>
        /// 售前需求申请流程关联单据
        /// </summary>
        public List<BeforeSaleDemandOrders> BeforeSaleDemandOrders  { get; set; }
        /// <summary>
        /// 售前需求申请流程附件
        /// </summary>
        public List<BeforeSaleFiles> BeforeSaleFiles { get; set; }
        /// <summary>
        /// 图片文件列表
        /// </summary>
        [Ignore]
        public virtual List<UploadFileResp> Files { get; set; }

        /// <summary>
        /// 确定开发部门列表
        /// </summary>
        public List<BeforeSaleDemandDeptInfo> BeforeSaleDemandDeptInfos { get; set; }
        /// <summary>
        /// 判断当前用户是否有页面的审核权限 默认否false
        /// </summary>
        public bool IsHandle { get; set; }
        /// <summary>
        /// 判断当前用户是否有查看金额信息的权限 默认否false
        /// </summary>
        public bool IsShowAmount { get; set; }
        /// <summary>
        /// 售前申请流程关联项目
        /// </summary>
        public List<BeforeSaleDemandProject> Beforesaledemandprojects { get; set; }
        /// <summary>
        /// 项目排期表
        /// </summary>
        public List<BeforeSaleProScheduling> BeforeSaleProSchedulings { get; set; }
    }
}