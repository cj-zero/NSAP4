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
	/// 服务单
	/// </summary>
    [Table("serviceorder")]
    public partial class ServiceOrder
    {
        public ServiceOrder()
        {
            this.CustomerId = string.Empty;
            this.CustomerName = string.Empty;
            this.Contacter = string.Empty;
            this.ContactTel = string.Empty;
            this.Supervisor = string.Empty;
            this.SupervisorId = string.Empty;
            this.SalesMan = string.Empty;
            this.SalesManId = string.Empty;
            this.NewestContacter = string.Empty;
            this.NewestContactTel = string.Empty;
            this.TerminalCustomer = string.Empty;
            this.CreateTime = DateTime.Now;
            this.CreateUserId = string.Empty;
            this.Province = string.Empty;
            this.City = string.Empty;
            this.Addr = string.Empty;
            this.Area = string.Empty;
            this.Address = string.Empty;
            this.AddressDesignator = string.Empty;
            this.TerminalCustomerId = string.Empty;
            this.Remark = string.Empty;
            this.VestInOrg = 1;
        }

        /// <summary>
        /// 服务单Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 客户代码
        /// </summary>
        [Description("客户代码")]
        public string CustomerId { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        [Description("客户名称")]
        public string CustomerName { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        [Description("联系人")]
        public string Contacter { get; set; }
        /// <summary>
        /// 联系人电话
        /// </summary>
        [Description("联系人电话")]
        public string ContactTel { get; set; }
        /// <summary>
        /// 主管名字
        /// </summary>
        [Description("主管名字")]
        public string Supervisor { get; set; }
        /// <summary>
        /// 主管用户Id
        /// </summary>
        [Description("主管用户Id")]
        [Browsable(false)]
        public string SupervisorId { get; set; }
        /// <summary>
        /// 销售名字
        /// </summary>
        [Description("销售名字")]
        public string SalesMan { get; set; }
        /// <summary>
        /// 销售用户Id
        /// </summary>
        [Description("销售用户Id")]
        [Browsable(false)]
        public string SalesManId { get; set; }
        /// <summary>
        /// 最新联系人
        /// </summary>
        [Description("最新联系人")]
        public string NewestContacter { get; set; }
        /// <summary>
        /// 最新联系人电话号码
        /// </summary>
        [Description("最新联系人电话号码")]
        public string NewestContactTel { get; set; }

        /// <summary>
        /// 终端客户代码
        /// </summary>
        [Description("终端客户代码")]
        public string TerminalCustomerId { get; set; }

        /// <summary>
        /// 终端客户
        /// </summary>
        [Description("终端客户")]
        public string TerminalCustomer { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public System.DateTime? CreateTime { get; set; }
        /// <summary>
        /// 创建人Id
        /// </summary>
        [Description("创建人Id")]
        [Browsable(false)]
        public string CreateUserId { get; set; }
        /// <summary>
        /// App用户Id
        /// </summary>
        [Description("App用户Id")]
        //[Browsable(false)]
        public int? AppUserId { get; set; }
        /// <summary>
        /// 接单人用户Id
        /// </summary>
        [Description("接单人用户Id")]
        [Browsable(false)]
        public string RecepUserId { get; set; }
        /// <summary>
        /// 接单人姓名
        /// </summary>
        [Description("接单人姓名")]
        public string RecepUserName { get; set; }
        /// <summary>
        /// 服务单状态 1-待确认 2-已确认 3-已取消
        /// </summary>
        [Description("服务单状态")]
        public int Status { get; set; }
        /// <summary>
        /// App技术主管Id
        /// </summary>
        [Description("App技术主管Id")]
        //[Browsable(false)]
        public int? ManagerId { get; set; }
        /// <summary>
        /// 是否关单
        /// </summary>
        [Browsable(false)]
        public bool IsClose { get; set; }
        /// <summary>
        /// 是否修改过
        /// </summary>
        public bool IsModified { get; set; }

        /// <summary>
        /// 地址标识
        /// </summary>
        [Description("地址标识")]
        public string AddressDesignator { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        [Description("地址")]
        public string Address { get; set; }
        /// <summary>
        /// 服务内容
        /// </summary>
        [Description("服务内容")]
        public string Services { get; set; }

        /// <summary>
        /// 省
        /// </summary>
        [Description("省")]
        public string Province { get; set; }
        /// <summary>
        /// 市
        /// </summary>
        [Description("市")]
        public string City { get; set; }
        /// <summary>
        /// 区
        /// </summary>
        [Description("区")]
        public string Area { get; set; }
        /// <summary>
        /// 详细地址
        /// </summary>
        [Description("详细地址")]
        public string Addr { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        [Description("经度")]
        public decimal? Longitude { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        [Description("纬度")]
        public decimal? Latitude { get; set; }
        /// <summary>
        /// 呼叫来源  呼叫来源  1-电话 2-钉钉 3-QQ 4-微信 5-邮件 6-APP 7-Web
        /// </summary>
        [Description("呼叫来源 1-电话 2-钉钉 3-QQ 4-微信 5-邮件 6-APP 7-Web 8-ECN")]
        //[Browsable(false)]
        public int? FromId { get; set; }

        /// <summary>
        /// SAP 服务单ID
        /// </summary>
        [Description("SAP 服务单ID ")]
        public int? U_SAP_ID { get; set; }

        /// <summary>
        /// 问题类型Id
        /// </summary>
        [Description("问题类型Id")]
        public string ProblemTypeId { get; set; }

        /// <summary>
        /// 问题类型名称
        /// </summary>
        [Description("问题类型名称")]
        public string ProblemTypeName { get; set; }

        /// <summary>
        /// 撤回备注
        /// </summary>
        [Description("撤回备注")]
        public string Remark { get; set; }
        
        /// <summary>
        /// 归属部门
        /// </summary>
        [Description("归属部门")]
        public int? VestInOrg { get; set; }
        /// <summary>
        /// app客户id
        /// </summary>
        [Description("app客户id")]
        public int? FromAppUserId { get; set; }
        /// <summary>
        /// 是否允许服务
        /// </summary>
        [Description("是否允许服务")]
        public int AllowOrNot { get; set; }
        /// <summary>
        /// 预计服务方式
        /// </summary>
        public int? ExpectServiceMode { get; set; }
        /// <summary>
        /// 预计服务方式占比
        /// </summary>
        public decimal? ExpectRatio { get; set; }

        /// <summary>
        /// 接单员部门
        /// </summary>
        [NotMapped]
        public string RecepUserDept { get; set; }
        /// <summary>
        /// 售后审核部门
        /// </summary>
        [NotMapped]
        public string SalesManDept { get; set; }
        /// <summary>
        /// 业务员部门
        /// </summary>
        [NotMapped]
        public string SuperVisorDept { get; set; }

        /// <summary>
        /// 服务单关联的工单
        /// </summary>
        public virtual List<ServiceWorkOrder> ServiceWorkOrders { get; set; }
        /// <summary>
        /// 服务单关联的图片
        /// </summary>
        [Ignore]
        public virtual List<ServiceOrderPicture> ServiceOrderPictures { get; set; }

        /// <summary>
        /// 服务单关联制作商序列号，物料
        /// </summary>
        public virtual List<ServiceOrderSerial> ServiceOrderSNs { get; set; }

        /// <summary>
        /// 服务单关联的售后流程
        /// </summary>
        public virtual List<ServiceFlow> ServiceFlows { get; set; }
    }
    

    public class ProcessingEfficiency
    {
        /// <summary>
        /// 部门主管
        /// </summary>
        public string SuperVisor { get; set; }
        /// <summary>
        /// 部门
        /// </summary>
        [NotMapped]
        public string Dept { get; set; }
        /// <summary>
        /// 已完成的服务单数量
        /// </summary>
        public int FinishCount { get; set; }
        /// <summary>
        /// 未完成的服务单数量
        /// </summary>
        [NotMapped]
        public int UnFinishCount { get; set; }
        /// <summary>
        /// 处理时间1天内
        /// </summary>
        public int D1 { get; set; }
        /// <summary>
        /// 处理时间2天
        /// </summary>
        public int D2 { get; set; }
        /// <summary>
        /// 处理时间3天
        /// </summary>
        public int D3 { get; set; }
        /// <summary>
        /// 处理时间4天
        /// </summary>
        public int D4 { get; set; }
        /// <summary>
        /// 处理时间5天
        /// </summary>
        public int D5 { get; set; }
        /// <summary>
        /// 处理时间6天
        /// </summary>
        public int D6 { get; set; }
        /// <summary>
        /// 处理时间7-14天
        /// </summary>
        public int D7_14 { get; set; }
        /// <summary>
        /// 处理时间15-30天
        /// </summary>
        public int D15_30 { get; set; }
        /// <summary>
        /// 处理时间超过30天
        /// </summary>
        public int D30 { get; set; }
    }

    public class ProcessingEfficiency2
    {
        /// <summary>
        /// 部门主管
        /// </summary>
        public string SuperVisor { get; set; }
        /// <summary>
        /// 部门
        /// </summary>
        [NotMapped]
        public string Dept { get; set; }
        /// <summary>
        /// 已完成的服务单数量
        /// </summary>
        public int FinishCount { get; set; }
        /// <summary>
        /// 处理时间24小时内
        /// </summary>
        public int D1 { get; set; }
        /// <summary>
        /// 处理时间48小时内
        /// </summary>
        public int D2 { get; set; }
        /// <summary>
        /// 处理时间72小时内
        /// </summary>
        public int D3 { get; set; }
        /// <summary>
        /// 处理时间72小时外
        /// </summary>
        public int D4 { get; set; }
    }

    public class ProblemTypeMonth
    {
        /// <summary>
        /// 问题类型
        /// </summary>
        public string ProblemTypeId { get; set; }

        /// <summary>
        /// 月份
        /// </summary>
        public string Months { get; set; }

        /// <summary>
        /// 个数
        /// </summary>
        public int Num { get; set; }
    }

    public class ServiceOrderData
    {
        public int? Id { get; set; }
        //public int status { get; set; }
        //public DateTime starttime { get; set; }
        //public DateTime? endtime { get; set; }
        //public string? supervisor { get; set; }
    }
}