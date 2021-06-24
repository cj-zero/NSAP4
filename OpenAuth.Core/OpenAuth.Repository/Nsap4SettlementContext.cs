using System;
using System.Collections.Generic;
using System.Text;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Settlement;

namespace OpenAuth.Repository
{
    [ConnectionString("Nsap4SettlementContext")]
    public  class Nsap4SettlementContext : DbContext
    {
        public Nsap4SettlementContext(DbContextOptions<Nsap4SettlementContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //当主键为联合主键时，需要把这里的内容拷贝到对应的位置
        }

        public virtual DbSet<outsourc> Outsourcs { get; set; }
        public virtual DbSet<outsourcexpenses> Outsourcexpenses { get; set; }
        public virtual DbSet<outsourcexpensespicture> Outsourcexpensespictures { get; set; }
    }
}
