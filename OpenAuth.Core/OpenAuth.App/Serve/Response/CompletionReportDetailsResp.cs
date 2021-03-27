using AutoMapper.Configuration.Annotations;
using Infrastructure.AutoMapper;
using OpenAuth.App.Serve.Response;
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
        /// 完工报告Id
        /// </summary>
        public string Id { get; set; }
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
        /// 终端客户Id
        /// </summary>
        public string TerminalCustomerId { get; set; }
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
        [Ignore]
        public virtual List<UploadFileResp> Files { get; set; }
        /// <summary>
        /// 工单对应的APP 技术员ID
        /// </summary>
        public int? CurrentUserId { get; set; }
        /// <summary>
        /// 工单对应的 NSAP 技术员信息
        /// </summary>
        public UserView TheNsapUser { get; set; }

        /// <summary>
        /// 工单对应留言信息
        /// </summary>
        public virtual List<ServiceOrderMessage> ServieOrderMsgs
        {
            get; set;
        }
        /// <summary>
        /// 故障描述
        /// </summary>
        public string TroubleDescription { get; set; }
        /// <summary>
        /// 过程描述
        /// </summary>
        public string ProcessDescription { get; set; }

        /// <summary>
        /// 服务方式
        /// </summary>
        public int? ServiceMode { get; set; }
        /// <summary>
        /// 工单
        /// </summary>
        public virtual List<WorkCompletionReportResp> ServiceWorkOrders { get; set; }

        /// <summary>
        /// 设备类型名称
        /// </summary>
        public string MaterialCodeTypeName { get; set; }

        /// 出发省
        /// </summary>
        public int? StartProvinceId { get; set; }

        /// <summary>
        /// 出发市
        /// </summary>
        public int? StartCityId { get; set; }

        /// <summary>
        /// 出发 区/县
        /// </summary>
        public int? StartAreaId { get; set; }

        /// <summary>
        /// 到达省
        /// </summary>
        public int? ArriveProvinceId { get; set; }

        /// <summary>
        /// 到达市
        /// </summary>
        public int? ArriveCityId { get; set; }
        /// <summary>
        /// 到达 区/县
        /// </summary>
        public int? ArriveAreaId { get; set; }

        /// <summary>
        /// U_SAP_ID
        /// </summary>
        public string U_SAP_ID { get; set; }

        /// <summary>
        /// 责任环节
        /// </summary>
        public string Responsibility { get; set; }

        /// <summary>
        /// 日报数量
        /// </summary>
        public int DailyReportNum { get; set; }
    }
}
