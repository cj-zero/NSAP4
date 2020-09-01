using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Response
{
    [AutoMapTo(typeof(ServiceWorkOrder))]
    public class WorkCompletionReportResp
    {
        /// <summary>
        /// 工单号
        /// </summary>
        public string WorkOrderNumber { get; set; }
        /// <summary>
        /// 制造商序列号
        /// </summary>
        public string ManufacturerSerialNumber { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }
    }
}
