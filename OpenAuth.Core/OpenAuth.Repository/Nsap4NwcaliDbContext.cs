using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository.Domain;

namespace OpenAuth.Repository
{
    [ConnectionString("Nsap4NwcaliDbContext")]
    public partial class Nsap4NwcaliDbContext : DbContext
    {

        public Nsap4NwcaliDbContext(DbContextOptions<Nsap4NwcaliDbContext> options)
            : base(options)
        { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //当主键为联合主键时，需要把这里的内容拷贝到对应的位置
            modelBuilder.Entity<Asset>().HasKey(c => new { c.Id });
            modelBuilder.Entity<Asset>(options => {
                options.Ignore(e => e.AssetCategorys);
                options.Ignore(e => e.AssetOperations);
            });
        }

        public virtual DbSet<Certinfo> Certinfos { get; set; }
        public virtual DbSet<Certplc> Certplcs { get; set; }
        public virtual DbSet<Asset> Assets { get; set; }
        public virtual DbSet<AssetCategory> Assetcategories { get; set; }
        public virtual DbSet<AssetInspect> Assetinspects { get; set; }
        public virtual DbSet<AssetOperation> Assetoperations { get; set; }
        public virtual DbSet<CertOperationHistory> Certoperationhistories { get; set; }
        //非数据库表格
        public virtual DbQuery<SysTableColumn> SysTableColumns { get; set; }
    }
}