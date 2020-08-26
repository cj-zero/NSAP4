using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    [ExcelExporter(Name = "呼叫服务", TableStyle = "Medium2")]
    public class ServiceOrderExcelDto
    {
        [ExporterHeader(DisplayName = "服务单号")]
        public int? U_SAP_ID { get; set; }
        [ExporterHeader(DisplayName = "客户代码")]
        public string CustomerId { get; set; }
        [ExporterHeader(DisplayName = "客户名称")]
        public string CustomerName { get; set; }

        /// <summary>
        /// 终端客户代码
        /// </summary>
        [ExporterHeader(DisplayName = "终端客户代码")]
        public string TerminalCustomerId { get; set; }

        /// <summary>
        /// 终端客户
        /// </summary>
        [ExporterHeader(DisplayName = "终端客户")]
        public string TerminalCustomer { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        [ExporterHeader(DisplayName = "联系人")]
        public string Contacter { get; set; }
        /// <summary>
        /// 联系人电话
        /// </summary>
        [ExporterHeader(DisplayName = "联系人电话")]
        public string ContactTel { get; set; }
        /// <summary>
        /// 最新联系人
        /// </summary>
        [ExporterHeader(DisplayName = "最新联系人")]
        public string NewestContacter { get; set; }
        /// <summary>
        /// 最新联系人电话号码
        /// </summary>
        [ExporterHeader(DisplayName = "最新联系人电话号码")]
        public string NewestContactTel { get; set; }
        /// <summary>
        /// 客服备注
        /// </summary>
        [ExporterHeader(DisplayName = "客服备注")]
        public string Service { get; set; }
        /// <summary>
        /// 主管名字
        /// </summary>
        [ExporterHeader(DisplayName = "主管")]
        public string Supervisor { get; set; }
        /// <summary>
        /// 销售名字
        /// </summary>
        [ExporterHeader(DisplayName = "销售")]
        public string SalesMan { get; set; }
        /// <summary>
        /// 接单人姓名
        /// </summary>
        [ExporterHeader(DisplayName = "接单人")]
        public string RecepUserName { get; set; }
        /// <summary>
        /// 呼叫来源  1-电话 2-APP 
        /// </summary>
        [ExporterHeader(DisplayName = "呼叫来源")]
        public string FromId { get; set; }
        /// <summary>
        /// 服务单状态 1-待确认 2-已确认 3-已取消
        /// </summary>
        [ExporterHeader(DisplayName = "服务单状态")]
        public string Status { get; set; }
        /// <summary>
        /// 地址标识
        /// </summary>
        [ExporterHeader(DisplayName = "地址标识")]
        public string AddressDesignator { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        [ExporterHeader(DisplayName = "地址")]
        public string Address { get; set; }
        /// <summary>
        /// 工单号
        /// </summary>
        [ExporterHeader(DisplayName = "工单号")]
        public string WorkOrderNumber { get; set; }
        /// <summary>
        /// 呼叫主题
        /// </summary>
        /// </summary>
        [ExporterHeader(DisplayName = "呼叫主题")]
        public string FromTheme { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        [ExporterHeader(DisplayName = "物料编码")]
        public string MaterialCode { get; set; }
        /// <summary>
        /// 物料描述
        /// </summary>
        [ExporterHeader(DisplayName = "物料描述")]
        public string MaterialDescription { get; set; }
        /// <summary>
        /// 制造商序列号
        /// </summary>
        [ExporterHeader(DisplayName = "制造商序列号")]
        public string ManufacturerSerialNumber { get; set; }
        /// <summary>
        /// 内部序列号
        /// </summary>
        [ExporterHeader(DisplayName = "内部序列号")]
        public string InternalSerialNumber { get; set; }
        /// <summary>
        /// 保修结束日期
        /// </summary>
        [ExporterHeader(DisplayName = "保修结束日期")]
        public System.DateTime? WarrantyEndDate { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [ExporterHeader(DisplayName = "备注")]
        public string Remark { get; set; }
        /// <summary>
        /// 呼叫类型1-提交呼叫 2-在线解答（已解决）
        /// </summary>
        [ExporterHeader(DisplayName = "呼叫类型")]
        public string FromType { get; set; }
        /// <summary>
        /// 问题类型
        /// </summary>
        [ExporterHeader(DisplayName = "问题类型")]
        public string ProblemType { get; set; }
        /// <summary>
        /// 优先级 4-紧急 3-高 2-中 1-低
        /// </summary>
        [ExporterHeader(DisplayName = "优先级")]
        public string Priority { get; set; }
        /// <summary>
        /// 服务类型 1-免费 2-收费
        /// </summary>
        [ExporterHeader(DisplayName = "服务类型")]
        public string FeeType { get; set; }
        /// <summary>
        /// 呼叫状态  1-待处理 2-已排配 3-已外出 4-已挂起 5-已接收 6-已解决 7-已回访
        /// </summary>
        [ExporterHeader(DisplayName = "工单呼叫状态")]
        public string WorkOrderStatus { get; set; }
        /// <summary>
        /// 当前接单技术员名称
        /// </summary>
        [ExporterHeader(DisplayName = "技术员")]
        public string CurrentUser { get; set; }
        /// <summary>
        /// 工单提交时间
        /// </summary>
        [ExporterHeader(DisplayName = "工单提交时间")]
        public System.DateTime? SubmitDate { get; set; }
        /// <summary>
        /// 预约日期
        /// </summary>
        [ExporterHeader(DisplayName = "预约日期")]
        public System.DateTime? BookingDate { get; set; }
        /// <summary>
        /// 上门时间
        /// </summary>
        [ExporterHeader(DisplayName = "上门时间")]
        public System.DateTime? VisitTime { get; set; }
        /// <summary>
        /// 清算日期
        /// </summary>
        [ExporterHeader(DisplayName = "清算日期")]
        public System.DateTime? LiquidationDate { get; set; }

        /// <summary>
        /// 解决方案
        /// </summary>
        [ExporterHeader(DisplayName = "解决方案")]
        public string Solution { get; set; }
        /// <summary>
        /// 故障描述
        /// </summary>
        [ExporterHeader(DisplayName = "故障描述")]
        public string TroubleDescription { get; set; }
        /// <summary>
        /// 过程描述
        /// </summary>
        [ExporterHeader(DisplayName = "过程描述")]
        public string ProcessDescription { get; set; }
    }
}
