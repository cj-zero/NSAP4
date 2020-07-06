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
    }
}
