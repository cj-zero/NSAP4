using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Response
{
    /// <summary>
    /// 
    /// </summary>
    [ExcelExporter(Name = "校准明细", TableStyle = "None")]
    public class ExportCalibrationReportResp
    {
        [ExporterHeader(DisplayName = "任务ID")]
        public string taskSubId { get; set; }

        [ExporterHeader(DisplayName = "生产码")]
        public string generatorCode { get; set; }

        [ExporterHeader(DisplayName = "通道ID")]
        public string chlId { get; set; }

        [ExporterHeader(DisplayName = "校准开始时间")]
        public string beginTime { get; set; }

        [ExporterHeader(DisplayName = "校准结束时间")]
        public string endTime { get; set; }

        [ExporterHeader(DisplayName = "耗时(秒)")]
        public string duration { get; set; }

        [ExporterHeader(DisplayName = "部门")]
        public string orgName { get; set; }

        [ExporterHeader(DisplayName = "操作人")]
        public string userName { get; set; }

        [ExporterHeader(DisplayName = "下位机GUID")]
        public string lowGuid { get; set; }

        [ExporterHeader(DisplayName = "下位机版本")]
        public string lowVer { get; set; }

        [ExporterHeader(DisplayName = "校准结果")]
        public string conclusion { get; set; }

        [ExporterHeader(DisplayName = "校准动作")]
        public string taskType { get; set; }

        [ExporterHeader(DisplayName = "设备型号")]
        public string TesterModel { get; set; }

        [ExporterHeader(DisplayName = "序列号")]
        public string serialNo { get; set; }

        [ExporterHeader(DisplayName = "校准器")]
        public string assetInfo { get; set; }

        [ExporterHeader(DisplayName = "出证人")]
        public string Issuer { get; set; }

        [ExporterHeader(DisplayName = "销售单号")]
        public string salesOrder { get; set; }

        [ExporterHeader(DisplayName = "销售交货单号")]
        public string DeliveryNumber { get; set; }

        [ExporterHeader(DisplayName = "销售员")]
        public string Salesman { get; set; }
    }
}
