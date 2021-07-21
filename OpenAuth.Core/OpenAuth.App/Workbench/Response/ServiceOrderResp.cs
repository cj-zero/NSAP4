using Infrastructure.AutoMapper;
using OpenAuth.App.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Workbench.Response
{
    /// <summary>
    /// 服务单详情
    /// </summary>
    public class ServiceOrderResp
    {
        /// <summary>
        /// 服务单id
        /// </summary>
        public string ServiceOrderId { get; set; }
        /// <summary>
        /// 服务单SapId
        /// </summary>
        public string ServiceOrderSapId { get; set; }
        /// <summary>
        /// 终端客户
        /// </summary>
        public string TerminalCustomer { get; set; }
        /// <summary>
        /// 终端客户代码
        /// </summary>
        public string TerminalCustomerId { get; set; }
        /// <summary>
        /// 申请人
        /// </summary>
        public string Petitioner { get; set; }
        /// <summary>
        /// 申请人id
        /// </summary>
        public string PetitionerId { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 开始地址
        /// </summary>
        public string Becity { get; set; }

        /// <summary>
        /// 到达地址
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// 销售审核
        /// </summary>
        public string SalesMan { get; set; }
        /// <summary>
        /// 售后审核
        /// </summary>
        public string Supervisor { get; set; }
        /// <summary>
        /// 联系人电话
        /// </summary>
        public string NewestContactTel { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        public string NewestContacter { get; set; }

        /// <summary>
        /// 服务单工单
        /// </summary>
        public List<ServiceWorkOrderResp> ServiceWorkOrders { get; set; }

        /// <summary>
        /// 服务单日报
        /// </summary>
        public List<ServiceDailyReportResp> ServiceDailyReports { get; set; }
        /// <summary>
        /// 服务单日费
        /// </summary>
        public List<ServiceDailyExpendsResp> ServiceDailyExpends { get; set; }

    }
    /// <summary>
    /// 服务单工单
    /// </summary>
    public class ServiceWorkOrderResp 
    {
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 呼叫主题
        /// </summary>
        public string FromTheme { get; set; }
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
        /// 创建日期
        /// </summary>
        public string CreateTime { get; set; }
        /// <summary>
        /// 工单号
        /// </summary>
        public string WorkOrderNumber { get; set; }

    }
    /// <summary>
    /// 服务单日报
    /// </summary>
    public class ServiceDailyReportResp
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; }
        /// <summary>
        /// 序列号
        /// </summary>
        public string ManufacturerSerialNumber { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }
        /// <summary>
        /// 解决方案
        /// </summary>
        public List<string> ProcessDescription { get; set; }
        /// <summary>
        /// 问题类型
        /// </summary>
        public List<string> TroubleDescription { get; set; }

    }
    /// <summary>
    /// 服务单日费
    /// </summary>
    public class ServiceDailyExpendsResp
    {
        /// <summary>
        /// 费用类型
        /// </summary>
        public string FeeType { get; set; }
        /// <summary>
        /// 报销单据类型(1 差补费， 2 交通费， 3住宿费， 4 其他费)
        /// </summary>
        public int DailyExpenseType { get; set; }
        /// <summary>
        /// 序号
        /// </summary>
        public int? SerialNumber { get; set; }
        /// <summary>
        /// 交通类型
        /// </summary>
        public string TrafficType { get; set; }
        /// <summary>
        /// 交通工具
        /// </summary>
        public string Transport { get; set; }
        /// <summary>
        /// 出发地
        /// </summary>
        public string From { get; set; }
        /// <summary>
        /// 目的地
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// 出发地址经度
        /// </summary>
        public string FromLng { get; set; }

        /// <summary>
        /// 出发地址纬度
        /// </summary>
        public string FromLat { get; set; }

        /// <summary>
        /// 到达地址经度
        /// </summary>
        public string ToLng { get; set; }

        /// <summary>
        /// 到达地址纬度
        /// </summary>
        public string ToLat { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal? Money { get; set; }
        /// <summary>
        /// 发票号码
        /// </summary>
        public string InvoiceNumber { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public System.DateTime? CreateTime { get; set; }
        /// <summary>
        /// 天数
        /// </summary>
        public int? Days { get; set; }
        /// <summary>
        /// 总金额
        /// </summary>
        public decimal? TotalMoney { get; set; }
        /// <summary>
        /// 费用类别
        /// </summary>
        public string ExpenseCategory { get; set; }
        /// <summary>
        /// 创建用户Id
        /// </summary>
        public string CreateUserId { get; set; }
        /// <summary>
        /// 创建用户
        /// </summary>
        public string CreateUserName { get; set; }
        /// <summary>
        /// 开票日期
        /// </summary>
        public System.DateTime? InvoiceTime { get; set; }
        /// <summary>
        /// 开票单位
        /// </summary>
        public string SellerName { get; set; }

        /// <summary>
        /// 报销附件集合
        /// </summary>
        public List<FileResp> Files { get; set; }
    }

}
