using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Material;
using OpenAuth.Repository.Domain.Workbench;

namespace OpenAuth.Repository
{
    [ConnectionString("OpenAuthDBContext", DbType = "MySql")]
    public partial class OpenAuthDBContext : DbContext
    {

        public OpenAuthDBContext(DbContextOptions<OpenAuthDBContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataPrivilegeRule>()
                .HasKey(c => new { c.Id });
            modelBuilder.Entity<WorkbenchPending>()
                .HasKey(c => new { c.ApprovalNumber });

        }

        public virtual DbSet<Application> Applications { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<CategoryType> CategoryTypes { get; set; }
        public virtual DbSet<FlowInstance> FlowInstances { get; set; }
        public virtual DbSet<FlowInstanceOperationHistory> FlowInstanceOperationHistorys { get; set; }
        public virtual DbSet<FlowInstanceTransitionHistory> FlowInstanceTransitionHistorys { get; set; }
        public virtual DbSet<FlowScheme> FlowSchemes { get; set; }
        public virtual DbSet<Form> Forms { get; set; }
        public virtual DbSet<Module> Modules { get; set; }
        public virtual DbSet<ModuleElement> ModuleElements { get; set; }
        public virtual DbSet<ModuleField> ModuleFields { get; set; }
        public virtual DbSet<OpenAuth.Repository.Domain.Org> Orgs { get; set; }
        public virtual DbSet<Relevance> Relevances { get; set; }
        public virtual DbSet<Resource> Resources { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UploadFile> UploadFiles { get; set; }

        public virtual DbSet<FrmLeaveReq> FrmLeaveReqs { get; set; }

        public virtual DbSet<SysLog> SysLogs { get; set; }

        public virtual DbSet<SysMessage> SysMessages { get; set; }

        public virtual DbSet<DataPrivilegeRule> DataPrivilegeRules { get; set; }

        public virtual DbSet<WmsInboundOrderDtbl> WmsInboundOrderDtbls { get; set; }
        public virtual DbSet<WmsInboundOrderTbl> WmsInboundOrderTbls { get; set; }
        public virtual DbSet<OpenJob> OpenJobs { get; set; }

        public virtual DbSet<ModuleFlowScheme> Moduleflowschemes { get; set; }
        public virtual DbSet<Corp> Corps { get; set; }
        public virtual DbSet<AppUserMap> AppUserMaps { get; set; }

        public virtual DbSet<GlobalArea> GlobalAreas { get; set; }

        public virtual DbSet<AppUserBind> AppUserBinds { get; set; }
        public virtual DbSet<BuilderTable> BuilderTables { get; set; }
        public virtual DbSet<BuilderTableColumn> BuilderTableColumns { get; set; }
        public virtual DbSet<WorkbenchPending> WorkbenchPendings { get; set; }

        public virtual DbSet<NsapUserMap> NsapUserMaps { get; set; }
        public virtual DbSet<VersionsLog> VersionsLogs { get; set; }
        public virtual DbSet<ReportInfo> ReportInfos { get; set; }

        #region 钉钉
        public virtual DbSet<DDBindUser> DDBindUsers { get; set; }
        public virtual DbSet<DDUserMsg> DDUserMsgs { get; set; }
        public virtual DbSet<DDDepartMsg> DDDepartMsgs { get; set; }
        #endregion

        #region 客户中间商 终端关系维护表
        public virtual DbSet<ClientRelation> ClientRelations { get; set; }
        public virtual DbSet<ClientRelHistory> ClientRelHistories { get; set; }
        public virtual DbSet<JobClientRelation> JobClientRelations { get; set; }
        #endregion

        #region 工程部相关维护表
        public virtual DbSet<ManageAccountBind> ManageAccountBinds { get; set; }
        public virtual DbSet<RateDetail> RateDetails { get; set; }
        public virtual DbSet<RateAnnix> RateAnnix { get; set; }
        #endregion

        //非数据库表格
        public virtual DbQuery<SysTableColumn> SysTableColumns { get; set; }
    }
}
