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
          this.CustomerId= string.Empty;
          this.CustomerName= string.Empty;
          this.Contacter= string.Empty;
          this.ContactTel= string.Empty;
          this.Supervisor= string.Empty;
          this.SupervisorId= string.Empty;
          this.SalesMan= string.Empty;
          this.SalesManId= string.Empty;
          this.NewestContacter= string.Empty;
          this.NewestContactTel= string.Empty;
          this.TerminalCustomer= string.Empty;
          this.CreateTime= DateTime.Now;
          this.CreateUserId= string.Empty;
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
        /// 服务单关联的工单
        /// </summary>
        public virtual List<ServiceWorkOrder> ServiceWorkOrders { get; set; }
        /// <summary>
        /// 服务单关联的图片
        /// </summary>
        [Ignore]
        public virtual List<ServiceOrderPicture> ServiceOrderPictures { get; set; }
    }
}