using Infrastructure;
using Microsoft.EntityFrameworkCore;
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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //当主键为联合主键时，需要把这里的内容拷贝到对应的位置
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
        public virtual DbSet<InternalcontactMaterial> InternalcontactMaterials { get; set; }
        public virtual DbSet<CommonUsedMaterial> CommonUsedMaterials { get; set; }
    }
}
