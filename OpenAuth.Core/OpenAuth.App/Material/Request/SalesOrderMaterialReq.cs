using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    public partial class SalesOrderMaterialReq : PageReq
    {
        /// <summary>
        /// 销售单号
        /// </summary>
        public string SalesOrderId { get; set; }

        /// <summary>
        /// 客户
        /// </summary>
        public string CustomerCode { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }
        /// <summary>
        /// 业务员
        /// </summary>

        public string SalesMan { get; set; }
        public string ProjectNo { get; set; }
        public string ProduceNo { get; set; }
        /// <summary>
        /// 是否有项目号（ Y 有 N 没有）
        /// </summary>
        public string IsPro { get; set; }
        /// <summary>
        /// 是否有图纸文件（ Y 有 N 没有）
        /// </summary>
        public string IsDraw { get; set; }
        /// <summary>
        /// 排序方式（ASC DESC）
        /// </summary>
        public string sortorder { get; set; }
        /// <summary>
        /// 特殊要求
        /// </summary>
        public string custom_req { get; set; }
        /// <summary>
        /// 编码类别
        /// </summary>
        public string ItemTypeName { get; set; }
        /// <summary>
        /// 图纸编码
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 版本号
        /// </summary>
        public string VersionNo { get; set; }
        /// <summary>
        /// 是否有版本号（ Y 有 N 没有）
        /// </summary>
        public string IsVersionNo { get; set; }
        /// <summary>
        /// 样机/批量
        /// </summary>
        public string IsDemo { get; set; }
        /// <summary>
        /// 超时提醒
        /// </summary>
        public string TimeRemind { get; set; }

        public string SubmitNo { get; set; }
    }

    public class TaskViewReq : PageReq
    {
        public string Owner { get; set; }
        public string Number { get; set; }
        public string objnbs { get; set; }
        public string StageName { get; set; }
        public string fld005506 { get; set; }
        public string fld006314 { get; set; }
        public string complete { get; set; }
        public int? isFinished { get; set; }

        /// <summary>
        /// 考勤月份
        /// </summary>
        public string Month { get; set; }
        /// <summary>
        /// 考勤状态
        /// </summary>
        public string DutyFlag { get; set; }
        public string AssignedTo { get; set; }
        public DateTime? duedateStart { get; set; }
        public DateTime? duedateEnd { get; set; }
        public DateTime? AssignDateStart { get; set; }
        public DateTime? AssignDateEnd { get; set; }
    }

    public class statisticsTableSpec
    {
        public string code { get; set; }
        public List<statisticsTable> data { get; set; } = new List<statisticsTable>();
}

    public class statisticsTable
    {
        public string Owner { get; set; }
        public string Number { get; set; }
        public string objnbs { get; set; }
        public string StageName { get; set; }
        public string fld005506 { get; set; }
        public int? complete { get; set; }
        public bool? isFinished { get; set; }
        public string fld006314 { get; set; }
        public DateTime duedate { get; set; }
        public int? DueDays { get; set; }
        public string AssignedBy { get; set; }
        public string AssignedTo { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime AssignDate { get; set; }
        public DateTime startDate { get; set; }
        public DateTime Completedate { get; set; }
        public string Month { get; set; }
    }


    public class statisticsTableB
    {
        public double TaskId { get; set; }

        public double UserCreatedId { get; set; }

        public double OwenerId { get; set; }

        public string Subject { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime DueDate { get; set; }

        public int hasReminder { get; set; }

        public int StatusId { get; set; }

        public int PriorityId { get; set; }

        public int Complete { get; set; }

        public int isFinished { get; set; }

        public int isPrivate { get; set; }

        public int isDeleted { get; set; }


        public DateTime AssignDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public double AssignedBy { get; set; }

        public string CaseRecGuid { get; set; }

        public string RecordGuid { get; set; }

        public string TaskNBS { get; set; }

        public double TaskOwnerId { get; set; }

        public float TimeAllocated { get; set; }

        public DateTime CompleteTime { get; set; }

        public string ownername { get; set; }

        public int DueHours { get; set; }

        public int WorkHours { get; set; }

    }


    public class submitMonth 
    {
        public List<string> Number { get; set; }
        public string Month { get; set; }
    }

    public class withdarwSubmitReq
    {
        public List<string> Number { get; set; }
    }

    public class ScoringDetail
    {
        public string level { get; set; }
        public string name { get; set; }
        public decimal LowDifficulty { get; set; }
        public decimal MediumDifficulty { get; set; }
        public decimal HighDifficulty { get; set; }
        public decimal SuperDifficulty { get; set; }
        public decimal OnTime { get; set; }
        public decimal Delayed { get; set; }
    }
}
