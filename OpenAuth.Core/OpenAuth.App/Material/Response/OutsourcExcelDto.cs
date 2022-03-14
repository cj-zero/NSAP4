using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
using System;
using System.Collections.Generic;
using System.Text;


namespace OpenAuth.App.Response
{
    [ExcelExporter(Name = "已处理结算", TableStyle = "None")]
    public class OutsourcExcelDto
    {
        [ExporterHeader(DisplayName = "结算编号")]
        public int Id { get; set; }

        [ExporterHeader(DisplayName = "服务单号")]
        public int? ServiceOrderSapId { get; set; }

        [ExporterHeader(DisplayName = "客户代码")]
        public string CustomerId { get; set; }

        [ExporterHeader(DisplayName = "客户名称")]
        public string CustomerName { get; set; }

        [ExporterHeader(DisplayName = "售后归属")]
        public string? Supervisor { get; set; }

        [ExporterHeader(DisplayName = "远程服务费")]
        public decimal? ServiceFee { get; set; }

        [ExporterHeader(DisplayName = "上门工时费")]
        public decimal? WorkingHoursFee { get; set; }

        [ExporterHeader(DisplayName = "交通补贴")]
        public decimal? TransportationFee{ get; set; }

        [ExporterHeader(DisplayName = "住宿补贴")]
        public decimal? AccommodationFee { get; set; }

        [ExporterHeader(DisplayName = "金额")]
        public decimal? TotalMoney { get; set; }

        [ExporterHeader(DisplayName = "提交人")]
        public string CreateUser { get; set; }

        //[ExporterHeader(DisplayName = "部门")]
        //public string OrgName { get; set; }
    }
}
