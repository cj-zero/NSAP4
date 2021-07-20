using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Serve;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.Repository
{
    [ConnectionString("Nsap4ServeDbContext")]
    public class Nsap4ServeDbContext : DbContext
    {
        public Nsap4ServeDbContext(DbContextOptions<Nsap4ServeDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ReimburseInfo>(options =>
            {
                options.Ignore(e => e.ReimburseAttachments);
            });
        }
        public virtual DbSet<Solution> Solutions { get; set; }
        public virtual DbSet<ProblemType> Problemtypes { get; set; }
        public virtual DbSet<AttendanceClock> Attendanceclocks { get; set; }
        public virtual DbSet<AttendanceClockPicture> Attendanceclockpictures { get; set; }
        public virtual DbSet<CompletionReport> Completionreports { get; set; }
        public virtual DbSet<ChangeTheMaterial> ChangeTheMaterials { get; set; }
        
        public virtual DbSet<CompletionReportPicture> Completionreportpictures { get; set; }
        public virtual DbSet<ServiceOrder> Serviceorders { get; set; }
        public virtual DbSet<ServiceOrderMessage> Serviceordermessages { get; set; }
        public virtual DbSet<ServiceOrderMessagePicture> Serviceordermessagepictures { get; set; }
        public virtual DbSet<ServiceOrderMessageUser> Serviceordermessageusers { get; set; }
        public virtual DbSet<ServiceOrderPicture> Serviceorderpictures { get; set; }
        public virtual DbSet<ServiceWorkOrder> Serviceworkorders { get; set; }
        public virtual DbSet<ServiceOrderLog> Serviceorderlogs { get; set; }

        public virtual DbSet<ServiceOrderSerial> Serviceorderserials { get; set; }
        public virtual DbSet<AppServiceOrderLog> Appserviceorderlogs { get; set; }
        public virtual DbSet<ServiceEvaluate> Serviceevaluates { get; set; }
        public virtual DbSet<RealTimeLocation> Realtimelocations { get; set; }
        public virtual DbSet<SeviceTechnicianApplyOrder> Sevicetechnicianapplyorders { get; set; }

        public virtual DbSet<PersonProblemAndSolution> Personproblemandsolutions { get; set; }

        public virtual DbSet<ServiceDailyReport> Servicedailyreports { get; set; }

        public virtual DbSet<ServiceDailyExpends> Servicedailyexpends { get; set; }

        public virtual DbSet<DailyAttachment> Dailyattachments { get; set; }
        #region 报销
        public virtual DbSet<ReimburseAccommodationSubsidy> Reimburseaccommodationsubsidies { get; set; }
        public virtual DbSet<ReimburseAttachment> Reimburseattachments { get; set; }
        public virtual DbSet<ReimburseFare> Reimbursefares { get; set; }
        public virtual DbSet<ReimburseInfo> Reimburseinfos { get; set; }
        public virtual DbSet<ReimburseOtherCharges> Reimburseordercharges { get; set; }
        public virtual DbSet<ReimburseTravellingAllowance> Reimbursetravellingallowances { get; set; }
        public virtual DbSet<ReimurseOperationHistory> Reimurseoperationhistories { get; set; }
        public virtual DbSet<ReimburseExpenseOrg> ReimburseExpenseOrgs { get; set; }

        public virtual DbSet<ServiceOrderParticipationRecord> ServiceOrderParticipationRecords { get; set; }
        
        public virtual DbSet<MyExpends> Myexpends { get; set; }
        #endregion

        public virtual DbSet<KnowledgeBase> KnowledgeBases { get; set; }
        //非数据库表格
        public virtual DbQuery<SysTableColumn> SysTableColumns { get; set; }

        /// <summary>
        /// 售后流程
        /// </summary>
        public virtual DbSet<ServiceFlow> Serviceflows { get; set; }

        public virtual DbSet<Express> Expressages { get; set; }
        public virtual DbSet<ExpressPicture> Expressagepictures { get; set; }
        public virtual DbSet<ReturnRepair> Returnrepairs { get; set; }

        public virtual DbSet<ServiceRedeploy> Serviceredeploys { get; set; }
    }
}
