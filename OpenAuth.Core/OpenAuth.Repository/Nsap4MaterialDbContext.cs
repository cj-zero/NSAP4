using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Material;

namespace OpenAuth.Repository
{
    [ConnectionString("Nsap4MaterialDbContext")]
    public partial class Nsap4MaterialDbContext : DbContext
    {
        public Nsap4MaterialDbContext(DbContextOptions<Nsap4MaterialDbContext> options)
          : base(options)
        { }

        public static readonly LoggerFactory loggerFactory = new LoggerFactory(new[] { new DebugLoggerProvider() });
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseLoggerFactory(loggerFactory);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //当主键为联合主键时，需要把这里的内容拷贝到对应的位置
            modelBuilder.Entity<MaterialRange>().HasKey(s => s.ItemCode);
        }
        public virtual DbSet<MaterialType> Materialtypes { get; set; }
        public virtual DbSet<ServiceOins> ServiceOins { get; set; }
        //非数据库表格
        public virtual DbQuery<SysTableColumn> SysTableColumns { get; set; }

        #region 物料
        public virtual DbSet<SalesOrderWarrantyDate> SalesOrderWarrantyDates { get; set; }

        public virtual DbSet<Expressage> Expressages { get; set; }

        public virtual DbSet<ExpressagePicture> ExpressagePictures { get; set; }

        public virtual DbSet<Quotation> Quotations { get; set; }

        public virtual DbSet<QuotationMaterial> QuotationMaterials { get; set; }
        public virtual DbSet<QuotationMaterialPicture> QuotationMaterialPictures { get; set; }
        public virtual DbSet<QuotationProduct> QuotationProducts { get; set; }

        public virtual DbSet<QuotationMergeMaterial> QuotationMergeMaterials { get; set; }

        public virtual DbSet<ReturnNote> Returnnotes { get; set; }
        public virtual DbSet<ReturnNotePicture> ReturnNotePictures { get; set; }
        
        public virtual DbSet<ReturnNoteMaterial> ReturnNoteMaterial { get; set; }

        public virtual DbSet<ReturnNoteMaterialPicture> ReturnNoteMaterialPictures { get; set; }

        public virtual DbSet<QuotationOperationHistory> QuotationOperationHistories { get; set; }

        public virtual DbSet<MaterialPrice> MaterialPrices { get; set; }

        public virtual DbSet<MaterialsSettlement> MaterialsSettlements{ get; set; }

        public virtual DbSet<SalesOrderWarrantyDateRecord> SalesOrderWarrantyDateRecords { get; set; }

        public virtual DbSet<QuotationPicture> QuotationPictures { get; set; }

        public virtual DbSet<LogisticsRecord> LogisticsRecords { get; set; }
        public virtual DbSet<amountinarear> amountinarears { get; set; }
        public virtual DbSet<amountinarearlog> amountinarearlogs { get; set; }
        public virtual DbSet<ReturnNoteProduct> ReturnNoteProducts { get; set; }
        //public virtual DbSet<ReturnnoteMaterialNumber> ReturnnoteMaterialNumbers { get; set; }
        #endregion
        public virtual DbSet<InternalContact> InternalContacts { get; set; }
        public virtual DbSet<InternalContactAttchment> InternalContactAttchments { get; set; }
        public virtual DbSet<InternalContactBatchNumber> InternalContactBatchNumbers { get; set; }
        public virtual DbSet<InternalContactDeptInfo> InternalContactDeptInfos { get; set; }
        public virtual DbSet<MaterialReplaceRecord> MaterialReplaceRecords { get; set; }
        public virtual DbSet<InternalContactMaterial> InternalContactMaterials { get; set; }
        public virtual DbSet<InternalContactEmailLog> InternalContactEmailLogs { get; set; }
        public virtual DbSet<CommonUsedMaterial> CommonUsedMaterials { get; set; }
        public virtual DbSet<MaterialRange> MaterialRanges { get; set; }


        #region 中位机和下位机
        public virtual DbSet<ZWJSoftwareVersion> ZWJSoftwareVersions { get; set; }
        public virtual DbSet<ZWJHardware> ZWJHardwares { get; set; }
        public virtual DbSet<XWJSoftwareVersion> XWJSoftwareVersions { get; set; }
        public virtual DbSet<XWJHardware> XWJHardwares { get; set; }
        public virtual DbSet<TempVersion> TempVersions { get; set; }
        # endregion
        public virtual DbSet<FinishedMaterial> FinishedMaterials { get; set; }
        
        public virtual DbSet<InternalContactTask> InternalContactTasks { get; set; }
        public virtual DbSet<InternalContactProduction> InternalContactProduction { get; set; }
        public virtual DbSet<InternalContactServiceOrder> InternalContactServiceOrders { get; set; }
        public virtual DbSet<InternalContactTaskServiceOrder> InternalContactTaskServiceOrders { get; set; }
        public virtual DbSet<InternalContactProductionDetail> InternalContactProductionDetails { get; set; }
    }
}
