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
    [ExcelExporter(Name = "烤机记录", TableStyle = "None")]
    public class ExportBakingMachineRecordResp
    {
        [ExporterHeader(DisplayName = "销售单号")]
        public string OriginAbs { get; set; }


        [ExporterHeader(DisplayName = "产品编码")]
        public string GeneratorCode { get; set; }

        [ExporterHeader(DisplayName = "物料编码")]
        public string ItemCode { get; set; }

        [ExporterHeader(DisplayName = "部门")]
        public string Department { get; set; }

        [ExporterHeader(DisplayName = "操作人")]
        public string CreateUser { get; set; }

        [ExporterHeader(DisplayName = "任务ID")]
        public string TaskId { get; set; }

        [ExporterHeader(DisplayName = "中位机GUID")]
        public string MidGuid { get; set; }

        [ExporterHeader(DisplayName = "下位机GUID")]
        public string LowGuid { get; set; }

        [ExporterHeader(DisplayName = "设备ID")]
        public int DevUid { get; set; }

        [ExporterHeader(DisplayName = "单元ID")]
        public int UnitId { get; set; }

        [ExporterHeader(DisplayName = "通道ID")]
        public int ChlId { get; set; }

        [ExporterHeader(DisplayName = "测试ID")]
        public long TestId { get; set; }

        [ExporterHeader(DisplayName = "测试开始时间")]
        public string begin { get; set; }

        [ExporterHeader(DisplayName = "测试结束时间")]
        public string end { get; set; }

        [ExporterHeader(DisplayName = "烤机结果")]
        public string result { get; set; }

        [ExporterHeader(DisplayName = "耗电(Wh)")]
        public string power { get; set; }

        [ExporterHeader(DisplayName = "二氧化碳(mg)")]
        public string carbon { get; set; }

        [ExporterHeader(DisplayName = "烤机耗时(秒)")]
        public string duration { get; set; }

        [ExporterHeader(DisplayName = "PCBA SN码")]
        public string sn { get; set; }
    }
}
