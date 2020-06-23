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
        }

        public virtual DbSet<Certinfo> Certinfos { get; set; }
        public virtual DbSet<Certplc> Certplcs { get; set; }
    }
}