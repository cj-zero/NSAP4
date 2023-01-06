using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Customer;
using OpenAuth.Repository.Domain.ProductModel;
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

        public static readonly LoggerFactory loggerFactory = new LoggerFactory(new[] { new DebugLoggerProvider() });
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseLoggerFactory(loggerFactory);
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
        public virtual DbSet<AttendanceClockWhileList> AttendanceClockWhileLists { get; set; }
        public virtual DbSet<CompletionReport> Completionreports { get; set; }
        public virtual DbSet<ChangeTheMaterial> ChangeTheMaterials { get; set; }
        public virtual DbSet<RequestActionLog> RequestActionLogs { get; set; }

        public virtual DbSet<CompletionReportPicture> Completionreportpictures { get; set; }
        public virtual DbSet<ServiceOrder> Serviceorders { get; set; }
        public virtual DbSet<ServiceOrderMessage> Serviceordermessages { get; set; }
        public virtual DbSet<ServiceOrderMessagePicture> Serviceordermessagepictures { get; set; }
        public virtual DbSet<ServiceOrderMessageUser> Serviceordermessageusers { get; set; }
        public virtual DbSet<ServiceOrderPicture> Serviceorderpictures { get; set; }
        public virtual DbSet<ServiceWorkOrder> Serviceworkorders { get; set; }
        public virtual DbSet<ServiceOrderLog> Serviceorderlogs { get; set; }
        public virtual DbSet<ServiceUnCompletedReasonHistory> ServiceUnCompletedReasonHistories { get; set; }
        public virtual DbSet<ServiceUnCompletedReasonDetail> ServiceUnCompletedReasonDetails { get; set; }
        public virtual DbSet<BeforeSaleDemand> BeforeSaleDemands { get; set; }
        public virtual DbSet<BeforeSaleDemandOrders> Beforesaledemandorders { get; set; }
        public virtual DbSet<BeforeSaleDemandOperationHistory> BeforeSaleDemandOperationHistories { get; set; }
        public virtual DbSet<BeforeSaleDemandProject> BeforeSaleDemandProjects { get; set; }
        public virtual DbSet<BeforeSaleDemandDeptInfo> BeforeSaleDemandDeptInfos { get; set; }
        public virtual DbSet<BeforeSaleFiles> BeforeSaleFiles { get; set; }
        public virtual DbSet<BeforeSaleProScheduling> BeforeSaleProSchedulings { get; set; }
        public virtual DbSet<BlameBelong> BlameBelongs { get; set; }
        public virtual DbSet<BlameBelongFile> BlameBelongFiles { get; set; }
        public virtual DbSet<BlameBelongOrg> BlameBelongOrgs { get; set; }
        public virtual DbSet<BlameBelongOrgFile> BlameBelongOrgFiles { get; set; }
        public virtual DbSet<BlameBelongUser> BlameBelongUsers { get; set; }
        public virtual DbSet<BlameBelongUserFile> BlameBelongUserFiles { get; set; }
        

        public virtual DbSet<BlameBelongHistory> BlameBelongHistorys { get; set; }

        public virtual DbSet<ServiceOrderSerial> Serviceorderserials { get; set; }
        public virtual DbSet<AppServiceOrderLog> Appserviceorderlogs { get; set; }
        public virtual DbSet<ServiceEvaluate> Serviceevaluates { get; set; }
        public virtual DbSet<RealTimeLocation> Realtimelocations { get; set; }
        public virtual DbSet<CustomerLocation> CustomerLocations { get; set; }
        public virtual DbSet<LocationViewUser> LocationViewUsers { get; set; }
        public virtual DbSet<SeviceTechnicianApplyOrder> Sevicetechnicianapplyorders { get; set; }

        public virtual DbSet<PersonProblemAndSolution> Personproblemandsolutions { get; set; }

        public virtual DbSet<ServiceDailyReport> Servicedailyreports { get; set; }

        public virtual DbSet<ServiceDailyExpends> Servicedailyexpends { get; set; }

        public virtual DbSet<DailyAttachment> Dailyattachments { get; set; }
        public virtual DbSet<IncomeSummary> IncomeSummarys { get; set; }
        public virtual DbSet<TechnicianCompletedQuantity> TechnicianCompletedQuantitys { get; set; }
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
        public virtual DbQuery<ProcessingEfficiency> ProcessingEfficiencies { get; set; }
        public virtual DbQuery<ProcessingEfficiency2> ProcessingEfficiency2s { get; set; }
        public virtual DbQuery<ProblemTypeMonth> ProblemTypeMonths { get; set; }
        public virtual DbQuery<ServiceOrderData> ServiceOrderDatas { get; set; }
        /// <summary>
        /// 售后流程
        /// </summary>
        public virtual DbSet<ServiceFlow> Serviceflows { get; set; }

        public virtual DbSet<Express> Expressages { get; set; }
        public virtual DbSet<ExpressPicture> Expressagepictures { get; set; }
        public virtual DbSet<ReturnRepair> Returnrepairs { get; set; }

        public virtual DbSet<ServiceRedeploy> Serviceredeploys { get; set; }
        public virtual DbSet<ProductModelType> productmodeltype { get; set; }
        public virtual DbSet<SharingPartner> SharingPartners { get; set; }
        #region 展会
        public virtual DbSet<Meeting> Meeting { get; set; }
        public virtual DbSet<MeetingDispatch> MeetingDispatch { get; set; }
        public virtual DbSet<MeetingOpreateLog> MeetingOpreateLog { get; set; }
        public virtual DbSet<MeetingUser> MeetingUser { get; set; }
        public virtual DbSet<MeetingFile> MeetingFile { get; set; }
        public virtual DbSet<MeetingDraft> meetingdraft { get; set; }
        public virtual DbSet<MeetingDraftlog> meetingdraftlog { get; set; }

        #endregion
        public virtual DbSet<ProductModelCategory> productmodelcategory { get; set; }
        public virtual DbSet<ProductModelSelection> productmodelselections { get; set; }
        public virtual DbSet<ProductModelSelectionInfo> productmodelselectioninfo { get; set; }
        public virtual DbSet<FromThemeRelevant> FromThemeRelevants { get; set; }

        #region 客户
        public virtual DbSet<CustomerList> CustomerLists { get; set; }
        public virtual DbSet<CustomerLimit> CustomerLimit { get; set; }
        public virtual DbSet<CustomerLimitRule> CustomerLimitRule { get; set; }
        public virtual DbSet<CustomerLimitSaler> CustomerLimitSaler { get; set; }
        public virtual DbSet<SpecialCustomer> SpecialCustomers { get; set; }
        public virtual DbSet<CustomerSeaConf> CustomerSeaConfs { get; set; }
        public virtual DbSet<CustomerSeaRule> CustomerSeaRules { get; set; }
        public virtual DbSet<CustomerSeaRuleItem> CustomerSeaRuleItems { get; set; }
        public virtual DbSet<CustomerSalerHistory> CustomerSalerHistories { get; set; }
        public virtual DbSet<CustomerMoveHistory> CustomerMoveHistories { get; set; }
        public virtual DbSet<CustomerOperationRecord> CustomerOperationRecord { get; set; }
        public virtual DbSet<ClientFollowUp> ClientFollowUp { get; set; }
        public virtual DbSet<ClientFollowUpPhrase> ClientFollowUpPhrase { get; set; }
        public virtual DbSet<ClientSchedule> ClientSchedule { get; set; }

        public virtual DbSet<LimsInfo> LimsInfo { get; set; }
        public virtual DbSet<LimsInfoMap> LimsInfoMap { get; set; }
        //lims推广员维护客户联系人表
        public virtual DbSet<LimsOCPR> LimsOCPR { get; set; }
        //lims推广员维护客户地址表
        public virtual DbSet<LimsCRD1> LimsCRD1 { get; set; }
        #endregion
        #region 工程部项目筛选
        public virtual DbSet<ManageScreening> ManageScreening { get; set; }
        public virtual DbSet<ManageScreeningHistory> ManageScreeningHistory { get; set; }
        public virtual DbSet<TaskView> TaskView { get; set; }

        
        #endregion
        #region 线索
        public virtual DbSet<Clue> Clue { get; set; }
        public virtual DbSet<ClueContacts> ClueContacts { get; set; }
        public virtual DbSet<ClueFile> ClueFile { get; set; }
        public virtual DbSet<ClueFollowUp> ClueFollowUp { get; set; }
        public virtual DbSet<ClueIntentionProduct> ClueIntentionProduct { get; set; }
        public virtual DbSet<ClueLog> ClueLog { get; set; }
        public virtual DbSet<ClueSchedule> ClueSchedule { get; set; }
        public virtual DbSet<ClueClassification> ClueClassification { get; set; }
        #endregion

        #region 合同管理
        public virtual DbSet<ContractApply> contractapply { get; set; }
        public virtual DbSet<ContractFile> contractfile { get; set; }
        public virtual DbSet<ContractSign> contractsign { get; set; }
        public virtual DbSet<ContractOperationHistory> contractoperationhistory { get; set; }
        public virtual DbSet<ContractSeal> contractseal { get; set; }
        public virtual DbSet<ContractSealOperationHistory> contractsealoperationhistory { get; set; }
        public virtual DbSet<ContractTemplate> contracttemplate { get; set; }
        public virtual DbSet<ContractFileType> contractfiletype { get; set; }
        public virtual DbSet<ContractDownLoadFileHis> contractdownloadfilehis { get; set; }
        #endregion

        #region 新威字典
        public virtual DbSet<NewareDictionary> newaredictionary { get; set; }
        #endregion

        #region 付款条件
        public virtual DbSet<PayTermSet> paytermset { get; set; }
        public virtual DbSet<PayPhase> payphase { get; set; }
        public virtual DbSet<PayTermSave> paytermsave { get; set; }
        public virtual DbSet<PayUserRate> payuserate { get; set; }
        public virtual DbSet<PayLimitRule> paylimitrule { get; set; }
        public virtual DbSet<PayLimitRuleDetail> paylimitruledetail { get; set; }
        public virtual DbSet<PayAutoFreeze> payautofreeze { get; set; }
        public virtual DbSet<PayVIPCustomer> payvipcustomer { get; set; }
        public virtual DbSet<PayFreezeCustomer> payFreezeCustomer { get; set; }
        public virtual DbSet<PayWillFreezeCustomer> paywillfreezecustomer { get; set; }
        #endregion

        #region 消息推送记录
        public virtual DbSet<DDSendMsgHitory> ddsendmsghistory { get; set; }
        #endregion

        #region 意见反馈
        public virtual DbSet<ProblemFeedback> problemFeddback { get; set; }
        public virtual DbSet<ProblemFeedFile> problemFeedFile { get; set; }
        public virtual DbSet<ProblemFile> problemFile { get; set; }
         #endregion
    }
}
