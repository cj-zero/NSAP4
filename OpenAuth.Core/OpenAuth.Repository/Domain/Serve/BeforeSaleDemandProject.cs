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
	/// 售前需求对接项目表
	/// </summary>
    [Table("beforesaledemandproject")]
    public partial class BeforeSaleDemandProject : BaseEntity<int>
    {
        public BeforeSaleDemandProject()
        {
          this.BeforeSaleDemandId = 0;
          this.ProjectName = string.Empty;
          this.ProjectNum= string.Empty;
          this.PromoterId= string.Empty;
          this.PromoterName= string.Empty;
          this.ReqUserId= string.Empty;
          this.ReqUserName= string.Empty;
          this.DevUserId= string.Empty;
          this.DevUserName= string.Empty;
          this.TestUserId= string.Empty;
          this.TestUserName= string.Empty;
          this.FlowInstanceId= string.Empty;
          this.Status= 0;
          this.ProjectUrl= string.Empty;
          this.ProjectDocURL= string.Empty;
          this.CreateUserName= string.Empty;
          this.CreateUserId= string.Empty;
          this.CreateTime= DateTime.Now;
          this.UpdateTime= DateTime.Now;
        }

        /// <summary>
        /// 售前申请流程Id
        /// </summary>
        [Description("售前申请流程Id")]
        [Browsable(false)]
        public int? BeforeSaleDemandId { get; set; }

        /// <summary>
        /// 售前需求项目名称
        /// </summary>
        [Description("售前需求项目名称")]
        public string ProjectName { get; set; }
        /// <summary>
        /// 售前需求项目编号
        /// </summary>
        [Description("售前需求项目编号")]
        public string ProjectNum { get; set; }
        /// <summary>
        /// 发起人Id
        /// </summary>
        [Description("发起人Id")]
        [Browsable(false)]
        public string PromoterId { get; set; }
        /// <summary>
        /// 发起人
        /// </summary>
        [Description("发起人")]
        public string PromoterName { get; set; }
        /// <summary>
        /// 需求负责人Id
        /// </summary>
        [Description("需求负责人Id")]
        [Browsable(false)]
        public string ReqUserId { get; set; }
        /// <summary>
        /// 需求负责人名字
        /// </summary>
        [Description("需求负责人名字")]
        public string ReqUserName { get; set; }
        /// <summary>
        /// 研发负责人Id
        /// </summary>
        [Description("研发负责人Id")]
        [Browsable(false)]
        public string DevUserId { get; set; }
        /// <summary>
        /// 研发负责人
        /// </summary>
        [Description("研发负责人")]
        public string DevUserName { get; set; }
        /// <summary>
        /// 测试负责人Id
        /// </summary>
        [Description("测试负责人Id")]
        [Browsable(false)]
        public string TestUserId { get; set; }
        /// <summary>
        /// 测试负责人
        /// </summary>
        [Description("测试负责人")]
        public string TestUserName { get; set; }
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
        /// 审批流程Id
        /// </summary>
        [Description("审批流程Id")]
        [Browsable(false)]
        public string FlowInstanceId { get; set; }
        /// <summary>
        /// 状态 1-立项 2-需求 3-开发 4-测试5-实施 6-验收 7-结束
        /// </summary>
        [Description("状态 1-立项 2-需求 3-开发 4-测试5-实施 6-验收 7-结束")]
        public int Status { get; set; }
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
        /// 创建人名称
        /// </summary>
        [Description("创建人名称")]
        public string CreateUserName { get; set; }
        /// <summary>
        /// 创建人Id
        /// </summary>
        [Description("创建人Id")]
        [Browsable(false)]
        public string CreateUserId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public System.DateTime? CreateTime { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        [Description("更新时间")]
        public System.DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 项目排期列表
        /// </summary>
        public virtual List<BeforeSaleProScheduling> BeforeSaleProSchedulings { get; set; }
        public override void GenerateDefaultKeyVal()
        {
        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}