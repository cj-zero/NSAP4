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
using OpenAuth.Repository.Domain;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
	/// 完工报告表
	/// </summary>
    [Table("completionreport")]
    public partial class CompletionReport : Entity
    {
        public CompletionReport()
        {
            this.FromTheme = string.Empty;
            this.CustomerId = string.Empty;
            this.CustomerName = string.Empty;
            this.Contacter = string.Empty;
            this.ContactTel = string.Empty;
            this.TerminalCustomer = string.Empty;
            this.MaterialCode = string.Empty;
            this.ManufacturerSerialNumber = string.Empty;
            this.TechnicianId = string.Empty;
            this.TechnicianName = string.Empty;
            this.BusinessTripDate = DateTime.Now;
            this.EndDate = DateTime.Now;
            this.Becity = string.Empty;
            this.Destination = string.Empty;
            this.CompleteAddress = string.Empty;
            this.ProblemDescription = string.Empty;
            this.SolutionId = string.Empty;
            this.ReplacementMaterialDetails = string.Empty;
            this.Legacy = string.Empty;
            this.Remark = string.Empty;
            this.CreateTime = DateTime.Now;
            this.CreateUserId = string.Empty;
            this.TerminalCustomerId = string.Empty;
        }


        /// <summary>
        /// 服务工单Id
        /// </summary>
        [Description("服务工单Id")]
        [Browsable(false)]
        public int? ServiceWorkOrderId { get; set; }
        /// <summary>
        /// 服务单Id
        /// </summary>
        [Description("服务单Id")]
        [Browsable(false)]
        public int? ServiceOrderId { get; set; }
        /// <summary>
        /// 呼叫主题
        /// </summary>
        [Description("呼叫主题")]
        public string FromTheme { get; set; }
        /// <summary>
        /// 客户代码
        /// </summary>
        [Description("客户代码")]
        [Browsable(false)]
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
        /// 物料编码
        /// </summary>
        [Description("物料编码")]
        public string MaterialCode { get; set; }
        /// <summary>
        /// 制造商序列号
        /// </summary>
        [Description("制造商序列号")]
        public string ManufacturerSerialNumber { get; set; }
        /// <summary>
        /// 技术员Id
        /// </summary>
        [Description("技术员Id")]
        [Browsable(false)]
        public string TechnicianId { get; set; }
        /// <summary>
        /// 技术员名称
        /// </summary>
        [Description("技术员名称")]
        public string TechnicianName { get; set; }
        /// <summary>
        /// 出差时间
        /// </summary>
        [Description("出差时间")]
        public System.DateTime? BusinessTripDate { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        [Description("结束时间")]
        public System.DateTime? EndDate { get; set; }
        /// <summary>
        /// 出差天数
        /// </summary>
        [Description("出差天数")]
        public int? BusinessTripDays { get; set; }
        /// <summary>
        /// 出发地点
        /// </summary>
        [Description("出发地点")]
        public string Becity { get; set; }
        /// <summary>
        /// 到达地点
        /// </summary>
        [Description("到达地点")]
        public string Destination { get; set; }
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
        /// 完工地址
        /// </summary>
        [Description("完工地址")]
        public string CompleteAddress { get; set; }
        /// <summary>
        /// 问题描述及解决方案
        /// </summary>
        [Description("问题描述及解决方案")]
        public string ProblemDescription { get; set; }
        /// <summary>
        /// 解决方案Id
        /// </summary>
        [Description("解决方案Id")]
        [Browsable(false)]
        public string SolutionId { get; set; }
        /// <summary>
        /// 解决方案
        /// </summary>
        public virtual Solution Solution { get; set; }
        /// <summary>
        /// 更换物资明细
        /// </summary>
        [Description("更换物资明细")]
        public string ReplacementMaterialDetails { get; set; }
        /// <summary>
        /// 遗留问题
        /// </summary>
        [Description("遗留问题")]
        public string Legacy { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Description("备注")]
        public string Remark { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public System.DateTime? CreateTime { get; set; }
        /// <summary>
        /// 创建人ID
        /// </summary>
        [Description("创建人ID")]
        [Browsable(false)]
        public string CreateUserId { get; set; }

        /// <summary>
        /// 服务方式
        /// </summary>
        [Description("服务方式")]
        public int? ServiceMode { get; set; }
        
        /// 出发省
        /// </summary>
        [Description("出发省")]
        public int StartProvinceId { get; set; }

        /// <summary>
        /// 出发市
        /// </summary>
        [Description("出发市")]
        public int StartCityId { get; set; }

        /// <summary>
        /// 出发 区/县
        /// </summary>
        [Description("出发 区/县")]
        public int StartAreaId { get; set; }

        /// <summary>
        /// 到达省
        /// </summary>
        [Description("到达省")]
        public int ArriveProvinceId { get; set; }

        /// <summary>
        /// 到达省
        /// </summary>
        [Description("到达市")]
        public int ArriveCityId { get; set; }
        /// <summary>
        /// 到达省
        /// </summary>
        [Description("到达区/县")]
        public int ArriveAreaId { get; set; }

        /// <summary>
        /// 完工报告关联的图片
        /// </summary>
        [Ignore]
        public virtual List<CompletionReportPicture> CompletionReportPictures { get; set; }
    }
}