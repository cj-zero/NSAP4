using AutoMapper.Configuration.Annotations;
using Infrastructure.AutoMapper;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    [AutoMapTo(typeof(ServiceOrder))]
    public class ServiceOrderDetailsResp
    {
        /// <summary>
        /// 服务单Id
        /// </summary>
        public int Id { get; set; }

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
        /// 主管名字
        /// </summary>
        public string Supervisor { get; set; }
        /// <summary>
        /// 销售名字
        /// </summary>
        public string SalesMan { get; set; }
        /// <summary>
        /// 最新联系人
        /// </summary>
        public string NewestContacter { get; set; }
        /// <summary>
        /// 最新联系人电话号码
        /// </summary>
        public string NewestContactTel { get; set; }
        /// <summary>
        /// 终端客户
        /// </summary>
        public string TerminalCustomer { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public System.DateTime? CreateTime { get; set; }
        /// <summary>
        /// 接单人姓名
        /// </summary>
        public string RecepUserName { get; set; }

        /// <summary>
        /// 省
        /// </summary>
        public string Province { get; set; }
        /// <summary>
        /// 市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 区
        /// </summary>
        public string Area { get; set; }
        /// <summary>
        /// 详细地址
        /// </summary>
        public string Addr { get; set; }
        /// <summary>
        /// 服务内容
        /// </summary>
        public string Services { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        public decimal? Longitude { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public decimal? Latitude { get; set; }
        /// <summary>
        /// 问题类型Id
        /// </summary>
        public string ProblemTypeId { get; set; }

        /// <summary>
        /// 问题类型名称
        /// </summary>
        public string ProblemTypeName { get; set; }
        /// <summary>
        /// 服务单号
        /// </summary>
        public int? U_SAP_ID { get; set; }
        /// <summary>
        /// 服务单关联的工单
        /// </summary>
        public virtual List<ServiceWorkOrderDetailsResp> ServiceWorkOrders { get; set; }
        /// <summary>
        /// 图片文件列表
        /// </summary>
        [Ignore]
        public virtual List<UploadFileResp> Files { get; set; }

        public virtual List<ServiceOrderSerial> ServiceOrderSNs { get; set; }
    }
}
