using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository.Domain;
 
namespace OpenAuth.Repository
{
    [ConnectionString("Nsap4MaterialDbContext")]
    public partial class Nsap4MaterialDbContext:DbContext
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
        public virtual DbSet<Expressage> Expressages { get; set; }

        public virtual DbSet<ExpressagePicture> ExpressagePictures { get; set; }

        public virtual DbSet<Quotation> Quotations { get; set; }

        public virtual DbSet<QuotationMaterial> QuotationMaterials { get; set; }
        public virtual DbSet<QuotationProduct> QuotationProducts { get; set; }
        public virtual DbSet<ReturnNote> Returnnotes { get; set; }

        public virtual DbSet<ReturnnoteMaterial> ReturnnoteMaterials { get; set; }

        #endregion
    }
}
