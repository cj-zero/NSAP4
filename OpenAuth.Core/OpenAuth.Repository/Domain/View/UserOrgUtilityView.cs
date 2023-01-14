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

    public class SaleOrderUtilityView
    {

        public string _System_objNBS { get; set; }

        public string fld005506 { get; set; }

        public string fld006314 { get; set; }

        public string RecordGuid { get; set; }

        public int deleted { get; set; }

        public double _System_Progress { get; set; }

    }


    public class AlphaView
    {

        public string _System_objNBS { get; set; }

        public string fld005506 { get; set; }

        public string fld006314 { get; set; }

        public string RecordGuid { get; set; }

        public int deleted { get; set; }

        public double _System_Progress { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime DateModified { get; set; }

    }

    public class EchoView
    {

        public string submitNo { get; set; }

        public string docEntry { get; set; }

        public string CardCode { get; set; }

        public string U_ZS { get; set; }

        public string CardName { get; set; }

        public string itemCode { get; set; }

        public string itemDesc { get; set; }

        public int quantity { get; set; }

        public string slpName { get; set; }

        public DateTime submitTime { get; set; }

        public string contractReviewCode { get; set; }

        public string custom_req { get; set; }

        public string itemTypeName { get; set; }

        public string itemName { get; set; }

        public string versionNo { get; set; }

        public string projectNo { get; set; }

        public int process { get; set; }

        public string fileUrl { get; set; }

        public string urlUpdate { get; set; }

        public string isDemo { get; set; }

        public string demoUpdate { get; set; }

        public int produceNo { get; set; }

        public int rn { get; set; }
    }

    public class BetaView
    {

        public string _System_objNBS { get; set; }

        public string itemcode { get; set; }

        public string num { get; set; }

        public string IdRecord { get; set; }

        public string RecordGuid { get; set; }

        public int deleted { get; set; }

        public double _System_Progress { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime DateModified { get; set; }

    }

    public class BetaSubView
    {

        public int StageId { get; set; }

        public string Description { get; set; }

        public string LongDescription { get; set; }

        public int IndentLevel { get; set; }

        public DateTime Start { get; set; }

        public int Work { get; set; }

        public DateTime Finish { get; set; }

        public int OrderIndex { get; set; }

        public int CompletedWork { get; set; }

        public DateTime dueDate { get; set; }

        public int dueWork { get; set; }

        public int DueCompletedWork { get; set; }

        public string ResponsibleUser { get; set; }

        public string objNBS { get; set; }

        public string itemcode { get; set; }

        public double progress { get; set; }


    }


    public class TaskNbsView
    {
        public int TaskId { get; set; }
        public string TaskNBS { get; set; }
        public string Subject { get; set; }
        public string ProjectNo { get; set; }
    }

    public class BetaSubFinalView
    {

        public int StageId { get; set; }

        public string Description { get; set; }

        public string LongDescription { get; set; }

        public int IndentLevel { get; set; }

        public DateTime Start { get; set; }

        public int Work { get; set; }

        public DateTime Finish { get; set; }

        public int OrderIndex { get; set; }

        public int CompletedWork { get; set; }

        public DateTime dueDate { get; set; }

        public int dueWork { get; set; }

        public int DueCompletedWork { get; set; }

        public string ResponsibleUser { get; set; }

        public string objNBS { get; set; }

        public string itemcode { get; set; }

        public double progress { get; set; }

        public List<string> TaskList { get; set; } = new List<string>();

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


    public class LevelMDetails
    {
        public string objnbs { get; set; }
        public string itemcode { get; set; }
        public string num { get; set; }
        public string RecordGuid { get; set; }
        public string levelm { get; set; }
        public int deleted { get; set; }

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

    public class LegitCheckRequest
    {
        public List<string> checkList { get; set; }
    }

    public class BomRequest
    {
        public string ProductNo { get; set; }
    }

    public class BindUtilityUpdateRequest
    {

        public string MAccount { get; set; }

        public string MName { get; set; }

        public string LAccount { get; set; }

        public string LName { get; set; }

        public string Level { get; set; }

        public int DutyFlag { get; set; }
        public int IsDelete { get; set; }
    }


    public class DutyChartRequest
    {
        public string Month { get; set; }
        public int limit { get; set; }
        public int page { get; set; }
    }

    public class DutyChartResponse
    {
        public List<string> XData { get; set; }
        public List<SerieData> YData { get; set; } = new List<SerieData>();
    }

    public class SerieData
    {
        public string Name { get; set; }
        public List<decimal> SerieVal { get; set; } = new List<decimal>();
    }

    public class RateTableResponse
    {
        public string RateLevel { get; set; }
        public string Name { get; set; }
        public int FinishedTask { get; set; }
        public int DueTask { get; set; }
        public int ExcelTask { get; set; }

    }

    public class DutyDetailsRsp
    {
        public List<DutyDetails> ddr = new List<DutyDetails>();
        public int Total { get; set; }
    }


    public class DutyDetails
    {
        public string Name { get; set; }
        public string PartNum { get; set; }
        public string Theme { get; set; }
        public string TaskName { get; set; }
        public string ProductModel { get; set; }
        public string Completion { get; set; }
        public string DiffcultDegree { get; set; }
        public string Status { get; set; }
        public DateTime DueDate { get; set; }
        public string DueDays { get; set; }
        public string Assigner { get; set; }
        public string Assignee { get; set; }
        public string Creator { get; set; }
        public DateTime CreateDateTime { get; set; }
        public string Owner { get; set; }
        public DateTime AssignTime { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public string MainAnnex { get; set; }
    }

    public class RateTableDetailReq
    {
        public List<RateTableDetailExport> texports = new List<RateTableDetailExport>();
    }

    public class RateTableDetailExport
    {
        [ExporterHeader(DisplayName = "岗位级别")]
        public string DutyLevel { get; set; }
        [ExporterHeader(DisplayName = "姓名")]
        public string Name { get; set; }
        [ExporterHeader(DisplayName = "低难度(0.5件)")]
        public string Low { get; set; }
        [ExporterHeader(DisplayName = "中难度(件)")]
        public string Middle { get; set; }
        [ExporterHeader(DisplayName = "高难度(件*2)")]
        public string High { get; set; }
        [ExporterHeader(DisplayName = "超高难度(件*3)")]
        public string Super { get; set; }
        [ExporterHeader(DisplayName = "总件数")]
        public string TotalCount { get; set; }
        [ExporterHeader(DisplayName = "图纸完成量分值")]
        public string RateSocre { get; set; }
        [ExporterHeader(DisplayName = "按时完成")]
        public string Due { get; set; }
        [ExporterHeader(DisplayName = "延迟完成")]
        public string Delay { get; set; }
        [ExporterHeader(DisplayName = "总完成量")]
        public string Total { get; set; }
        [ExporterHeader(DisplayName = "按时完成率")]
        public string DueRatio { get; set; }
        [ExporterHeader(DisplayName = "按时完成分值")]
        public string DueScore { get; set; }
        [ExporterHeader(DisplayName = "超额完成(件)")]
        public string ExcelCount { get; set; }
        [ExporterHeader(DisplayName = "超额完成分值")]
        public string ExcelScore { get; set; }
    }

    public class DetailExportData
    {
        /// <summary>
        /// 数据
        /// </summary>
        public List<DetailExport> detports = new List<DetailExport>();
    }

    public class DetailExportSaveData
    {
        public string Time { get; set; }
        public int Flag { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public string detports { get; set; }
    }

    public class DetailData
    {
        public string Time { get; set; }
    }

    public class DetailDataA
    {
        public string Time { get; set; }
        public string Name { get; set; }
        public int limit { get; set; }

        public int page { get; set; }
    }


    public class MaterialDataReq
    {
        public List<string> Alpha { get; set; }
        public string ProjectNo { get; set; }
    }

    public class ArchiveData
    {
        public bool ArchiveFlag { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public RateDetail ArchiveDatas { get; set; }
    }

    public class ArchiveDataA
    {
        public int Count { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public List<RateAnnix> ArchiveDatas { get; set; } = new List<RateAnnix>();
    }

    /// <summary>
    /// 行数据
    /// </summary>
    public class DetailExport
    {
        /// <summary>
        /// 岗位级别
        /// </summary>
        public string DutyLevel { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 低难度(0.5件)
        /// </summary>
        public string Low { get; set; }
        /// <summary>
        /// 中难度(件)
        /// </summary>
        public string Middle { get; set; }
        /// <summary>
        /// 高难度(件*2)
        /// </summary>
        public string High { get; set; }
        /// <summary>
        /// 超高难度(件*3)
        /// </summary>
        public string Super { get; set; }
        /// <summary>
        /// 总件数
        /// </summary>
        public string TotalCount { get; set; }
        /// <summary>
        /// 图纸完成量分值
        /// </summary>
        public string RateSocre { get; set; }
        /// <summary>
        /// 按时完成
        /// </summary>
        public string Due { get; set; }
        /// <summary>
        /// 延迟完成
        /// </summary>
        public string Delay { get; set; }
        /// <summary>
        /// 总完成量
        /// </summary>
        public string Total { get; set; }
        /// <summary>
        /// 按时完成率
        /// </summary>
        public string DueRatio { get; set; }
        /// <summary>
        /// 按时完成分值
        /// </summary>
        public string DueScore { get; set; }
        /// <summary>
        /// 超额完成(件)
        /// </summary>
        public string ExcelCount { get; set; }
        /// <summary>
        /// 超额完成分值
        /// </summary>
        public string ExcelScore { get; set; }
    }


    public class RateTableReq
    {
        public List<RateTableExport> texports = new List<RateTableExport>();
    }

    public class RateAReq
    {
        public string Time { get; set; }
        public string Name { get; set; }
    }


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

    public class ArchieveExport
    {
        [ExporterHeader(DisplayName = "归档月份")]
        public string Month { get; set; }
        [ExporterHeader(DisplayName = "岗位级别")]
        public string Level { get; set; }
        [ExporterHeader(DisplayName = "姓名")]
        public string Name { get; set; }
        [ExporterHeader(DisplayName = "一般难度（件*0.5）")]
        public string LowDifficulty { get; set; }
        [ExporterHeader(DisplayName = "中等难度（件*1）")]
        public string MediumDifficulty { get; set; }
        [ExporterHeader(DisplayName = "高等难度（件*1）")]
        public string HighDifficulty { get; set; }
        [ExporterHeader(DisplayName = "特级难度（件*1）")]
        public string SuperDifficulty { get; set; }
        [ExporterHeader(DisplayName = "总件数")]
        public string Total { get; set; }
        [ExporterHeader(DisplayName = "图纸完成量分值（满分70分）")]
        public string TotalSc { get; set; }
        [ExporterHeader(DisplayName = "按时完成")]
        public string OnTime { get; set; }
        [ExporterHeader(DisplayName = "延迟完成")]
        public string Delayed { get; set; }
        [ExporterHeader(DisplayName = "总完成量")]
        public string TotalQ { get; set; }
        [ExporterHeader(DisplayName = "按时完成率")]
        public string OnTimePer { get; set; }
        [ExporterHeader(DisplayName = "按时完成分值（满分15分）")]
        public string OnTimeSc { get; set; }
        [ExporterHeader(DisplayName = "超额完成（件）")]
        public string OverQ { get; set; }
        [ExporterHeader(DisplayName = "超额完成（满分10分）")]
        public string OverSc { get; set; }
        [ExporterHeader(DisplayName = "岗位贡献（满分5分）")]
        public string Contribution { get; set; }
        [ExporterHeader(DisplayName = "分数")]
        public string Score { get; set; }
        [ExporterHeader(DisplayName = "生产评分")]
        public string Product { get; set; }
        [ExporterHeader(DisplayName = "总分")]
        public string TotalScore { get; set; }
        [ExporterHeader(DisplayName = "等级")]
        public string Rank { get; set; }

    }

    public class SerieManageData
    {
        public string AssignedTo { get; set; }

        public decimal Total { get; set; }

        public decimal CompleteCount { get; set; }

    }

    public class SerieManageDataRaw
    {
        public string AssignedTo { get; set; }

        public int CompleteCount { get; set; }

        public double LowDifficulty { get; set; }

        public double MediumDifficulty { get; set; }

        public double HighDifficulty { get; set; }

        public double SuperDifficulty { get; set; }


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


    public class RateAnnixSpec
    {
        public string Level { get; set; }

        public string Name { get; set; }

        public string LowDifficulty { get; set; }

        public string MediumDifficulty { get; set; }

        public string HighDifficulty { get; set; }

        public string SuperDifficulty { get; set; }

        public string OnTime { get; set; }

        public string Delayed { get; set; }

        public string Total { get; set; }

        public string TotalSc { get; set; }

        public string TotalQ { get; set; }

        public string OnTimePer { get; set; }

        public string OnTimeSc { get; set; }

        public string OverQ { get; set; }

        public string OverSc { get; set; }

        public string Score { get; set; }

        public string TotalScore { get; set; }

        public string Rank { get; set; }

        public string Contribution { get; set; }

        public string Product { get; set; }

        public string Month { get; set; }


    }




}
