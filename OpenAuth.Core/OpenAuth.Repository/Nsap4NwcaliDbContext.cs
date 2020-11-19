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
        public virtual DbSet<UserSign> UserSigns { get; set; }
        public virtual DbSet<Etalon> Etalons { get; set; }
        public virtual DbSet<NwcaliBaseInfo> Nwcalibaseinfos { get; set; }
        public virtual DbSet<NwcaliPlcData> Nwcaliplcdatas { get; set; }
        public virtual DbSet<NwcaliTur> Nwcaliturs { get; set; }
        public virtual DbSet<PcPlc> Pcplcs { get; set; }
    }
}