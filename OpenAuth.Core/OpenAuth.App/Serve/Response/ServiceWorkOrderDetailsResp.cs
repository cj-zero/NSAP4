﻿using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    [AutoMapTo(typeof(ServiceWorkOrder))]
    public class ServiceWorkOrderDetailsResp
    {
        /// <summary>
        /// 工单ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 服务单Id
        /// </summary>
        //[Browsable(false)]
        public int ServiceOrderId { get; set; }
        /// <summary>
        /// 优先级 4-紧急 3-高 2-中 1-低
        /// </summary>
        public int? Priority { get; set; }
        /// <summary>
        /// 服务类型 1-免费 2-收费
        /// </summary>
        public int? FeeType { get; set; }
        /// <summary>
        /// 省
        /// </summary>
        public string Province { get; set; }
        /// <summary>
        /// 市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 详细地址
        /// </summary>
        public string Addr { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        public decimal? Longitude { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public decimal? Latitude { get; set; }
        /// <summary>
        /// 工单提交时间
        /// </summary>
        public System.DateTime? SubmitDate { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 呼叫状态 1-待处理 2-已排配 3-已外出 4-已挂起 5-已接收 6-已解决 7-已回访
        /// </summary>
        public int? Status { get; set; }
        /// <summary>
        /// 呼叫主题
        /// </summary>
        public string FromTheme { get; set; }
        /// <summary>
        /// 呼叫来源  1-电话 2-APP 
        /// </summary>
        //[Browsable(false)]
        public int? FromId { get; set; }
        /// <summary>
        /// 问题类型Id
        /// </summary>
        //[Browsable(false)]
        public string ProblemTypeId { get; set; }
        /// <summary>
        /// 问题类型
        /// </summary>
        public virtual ProblemTypeDetailsResp ProblemType { get; set; }
        /// <summary>
        /// 呼叫类型1-提交呼叫 2-在线解答（已解决）
        /// </summary>
        public int? FromType { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }
        /// <summary>
        /// 物料描述
        /// </summary>
        public string MaterialDescription { get; set; }
        /// <summary>
        /// 制造商序列号
        /// </summary>
        public string ManufacturerSerialNumber { get; set; }
        /// <summary>
        /// 内部序列号
        /// </summary>
        public string InternalSerialNumber { get; set; }
        /// <summary>
        /// 保修结束日期
        /// </summary>
        public System.DateTime? WarrantyEndDate { get; set; }
        /// <summary>
        /// 创建日期
        /// </summary>
        public System.DateTime? CreateTime { get; set; }
        /// <summary>
        /// 预约日期
        /// </summary>
        public System.DateTime? BookingDate { get; set; }
        /// <summary>
        /// 上门时间
        /// </summary>
        public System.DateTime? VisitTime { get; set; }
        /// <summary>
        /// 清算日期
        /// </summary>
        public System.DateTime? LiquidationDate { get; set; }
        /// <summary>
        /// 解决方案
        /// </summary>
        public virtual SolutionDetailsResp Solution { get; set; }
        /// <summary>
        /// 地址标识
        /// </summary>
        public string AddressDesignator { get; set; }
        /// <summary>
        /// 完工报告
        /// </summary>
        public virtual CompletionReportDetailsResp CompletionReport { get; set; }
        /// <summary>
        /// 服务合同
        /// </summary>
        public string ContractId { get; set; }

        /// <summary>
        /// 故障描述
        /// </summary>
        public string TroubleDescription { get; set; }
        /// <summary>
        /// 过程描述
        /// </summary>
        public string ProcessDescription { get; set; }

        /// <summary>
        /// 服务方式 1上门服务 2电话服务
        /// </summary>
        public int? ServiceMode { get; set; }

    }
}
