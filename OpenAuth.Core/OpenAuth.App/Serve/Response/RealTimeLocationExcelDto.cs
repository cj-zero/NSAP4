using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    [ExcelExporter(Name = "定位数据", TableStyle ="None")]
    public class RealTimeLocationExcelDto
    {
        //[ExporterHeader(DisplayName ="部门")]
        //public string Org { get; set; }

        [ExporterHeader(DisplayName ="姓名")]
        public string Name { get; set; }


        [ExporterHeader(DisplayName = "所在省")]
        public string Province { get; set; }

        [ExporterHeader(DisplayName = "所在市")]
        public string City { get; set; }

        [ExporterHeader(DisplayName = "所在区")]
        public string Area { get; set; }

        [ExporterHeader(DisplayName = "详细地址")]
        public string Address { get; set; }

        [ExporterHeader(DisplayName = "日期")]
        public string CreateDate { get; set; }

        //[ExporterHeader(DisplayName = "出勤天数")]
        //public int Count { get; set; }
    }
}
