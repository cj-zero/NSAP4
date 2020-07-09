﻿using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    [AutoMapTo(typeof(CompletionReport))]
    public class CompletionReportDetailsResp
    {
        /// <summary>
        /// 服务工单Id
        /// </summary>
        public int? ServiceWorkOrderId { get; set; }
        /// <summary>
        /// 服务单Id
        /// </summary>
        public int? ServiceOrderId { get; set; }
        /// <summary>
        /// 呼叫主题
        /// </summary>
        public string FromTheme { get; set; }
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CustomerId { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        public string Contacter { get; set; }
        /// <summary>
        /// 联系人电话
        /// </summary>
        public string ContactTel { get; set; }
        /// <summary>
        /// 终端客户
        /// </summary>
        public string TerminalCustomer { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }
        /// <summary>
        /// 制造商序列号
        /// </summary>
        public string ManufacturerSerialNumber { get; set; }
        /// <summary>
        /// 技术员名称
        /// </summary>
        public string TechnicianName { get; set; }
        /// <summary>
        /// 出差时间
        /// </summary>
        public System.DateTime? BusinessTripDate { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public System.DateTime? EndDate { get; set; }
        /// <summary>
        /// 出差天数
        /// </summary>
        public int? BusinessTripDays { get; set; }
        /// <summary>
        /// 出发地点
        /// </summary>
        public string Becity { get; set; }
        /// <summary>
        /// 到达地点
        /// </summary>
        public string Destination { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        public decimal? Longitude { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public decimal? Latitude { get; set; }
        /// <summary>
        /// 完工地址
        /// </summary>
        public string CompleteAddress { get; set; }
        /// <summary>
        /// 问题描述及解决方案
        /// </summary>
        public string ProblemDescription { get; set; }
        /// <summary>
        /// 解决方案
        /// </summary>
        public virtual SolutionDetailsResp Solution { get; set; }
        /// <summary>
        /// 更换物资明细
        /// </summary>
        public string ReplacementMaterialDetails { get; set; }
        /// <summary>
        /// 遗留问题
        /// </summary>
        public string Legacy { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public System.DateTime? CreateTime { get; set; }
        /// <summary>
        /// 完工报告关联的图片
        /// </summary>
        public virtual List<UploadFileResp> CompletionReportPictures { get; set; }
    }
}