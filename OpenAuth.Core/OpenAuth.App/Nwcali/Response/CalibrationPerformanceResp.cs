using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;

namespace OpenAuth.App.Nwcali.Response
{
    /// <summary>
    /// 校准绩效明细
    /// </summary>
    [ExcelExporter(Name = "校准绩效明细", TableStyle = "None")]
    public class CalibrationPerformanceResp
    {
        [ExporterHeader(DisplayName = "校准人")]
        public string userName { get; set; }

        [ExporterHeader(DisplayName = "部门")]
        public string orgName { get; set; }

        [ExporterHeader(DisplayName = "设备数")]
        public string devCount { get; set; }

        [ExporterHeader(DisplayName = "成功次数")]
        public string taskOk { get; set; }

        [ExporterHeader(DisplayName = "失败次数")]
        public string taskNg { get; set; }

        [ExporterHeader(DisplayName = "自动校准次数")]
        public string taskAuto { get; set; }

        [ExporterHeader(DisplayName = "手动校准次数")]
        public string taskHd { get; set; }

        [ExporterHeader(DisplayName = "自动校准台数")]
        public string devAuto { get; set; }

        [ExporterHeader(DisplayName = "手动校准台数")]
        public string devHd { get; set; }

        [ExporterHeader(DisplayName = "自动校准耗时(秒)")]
        public string autoSpend { get; set; }

        [ExporterHeader(DisplayName = "手动校准耗时(秒)")]
        public string hdSpend { get; set; }

        [ExporterHeader(DisplayName = "自动校准平均耗时(秒/台)")]
        public string autoAvgSpend { get; set; }

        [ExporterHeader(DisplayName = "手动校准平均耗时(秒/台)")]
        public string hdAvgSpend { get; set; }
    }
}
