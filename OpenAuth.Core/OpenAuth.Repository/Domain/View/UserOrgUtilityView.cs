using DocumentFormat.OpenXml.Wordprocessing;
using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
using OpenAuth.Repository.Domain.Material;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.View
{

    public class UserOrgUtilityView
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Account { get; set; }

        public string deptName { get; set; }


    }

    public class UserManageUtilityRsp
    {
        public int Count { get; set; }

        public List<UserManageUtilityView> muv { get; set; } = new List<UserManageUtilityView>();
    }


    public class UserManageUtilityView
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Account { get; set; }

        public string deptName { get; set; }

        public int Status { get; set; }
    }

    public class UserManageUtilityRequest
    {

        public string SlpName { get; set; }

        public string deptName { get; set; }

        public int page { get; set; }

        public int Limit { get; set; }
    }

    public class BindUtilityRequest
    {

        public int page { get; set; }

        public int Limit { get; set; }

        public string query { get; set; }
    }

    public class BindUtilityUpdateRequest
    {

        public string MAccount { get; set; }

        public string MName { get; set; }

        public string LAccount { get; set; }

        public string LName { get; set; }

        public string  Level { get; set; }

        public int  DutyFlag { get; set; }
        public int IsDelete { get; set; }
    }


    public class DutyChartRequest
    { 
        public int Month { get; set; }
    }

    public class DutyChartResponse
    {
        public List<string> XData { get; set; }
        public List<SerieData> YData { get; set; } = new List<SerieData>();
    }

    public class SerieData
    {
        public string Name { get; set; }
        public List<double> SerieVal { get; set; }= new List<double>();
    }

    public class RateTableResponse
    {
        public string RateLevel { get; set; }
        public string Name { get; set; }
        public int FinishedTask { get; set; }
        public int DueTask { get; set; }
        public int ExcelTask { get; set; }

    }

    public class RateTableReq
    {
        public List<RateTableExport>  texports =new List<RateTableExport>();
    }

    [ExcelExporter(Name = "评分表", TableStyle = "Light10", AutoFitAllColumn = true, MaxRowNumberOnASheet = 2)]
    public class RateTableExport
    {
        [ExporterHeader(DisplayName = "岗位级别")]
        public string DutyLevel { get; set; }
        [ExporterHeader(DisplayName = "姓名")]
        public string Name { get; set; }
        [ExporterHeader(DisplayName = "图纸完成量(满分70分)")]
        public string FinishedTask { get; set; }
        [ExporterHeader(DisplayName = "按时完成(满分15分)")]
        public string DueTask { get; set; }
        [ExporterHeader(DisplayName = "超额完成(满分10分)")]
        public string ExcelTask { get; set; }
        [ExporterHeader(DisplayName = "岗位贡献(满分5分)")]
        public string DutyContribute { get; set; }
        [ExporterHeader(DisplayName = "分数")]
        public string Score { get; set; }
        [ExporterHeader(DisplayName = "生产评分")]
        public string RateScore { get; set; }
        [ExporterHeader(DisplayName = "总分")]
        public string TotalScore { get; set; }
        [ExporterHeader(DisplayName = "等级")]
        public string RateLevel { get; set; }
    }

    public class SerieManageData
    {
        public string Owner { get; set; }

        public int Total { get; set; }

        public int CompleteCount { get; set; }

    }

    public class BindUtilityRep
    {

        public int Count { get; set; }

        public List<ManageAccountBind> mab { get; set; } = new List<ManageAccountBind>();
    }


    public class MaterialUsers
    {
        public int UserID { get; set; }

        public string UserName { get; set; }

        public string FirstNameAndLastName { get; set; }
    }


}
