using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.Repository
{
    [ConnectionString("NsapBaseDbContext")]
    public class NsapBaseDbContext : DbContext
    {
        public NsapBaseDbContext(DbContextOptions<NsapBaseDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //当主键为联合主键时，需要把这里的内容拷贝到对应的位置
        }
        public virtual DbSet<base_user> BaseUsers { get; set; }
    }
}
