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

        public virtual DbSet<Outsourc> Outsourcs { get; set; }
        public virtual DbSet<OutsourcExpenses> Outsourcexpenses { get; set; }
        public virtual DbSet<OutsourcExpenseOrg> OutsourcExpenseOrgs { get; set; }
        public virtual DbSet<OutsourcExpensesPicture> Outsourcexpensespictures { get; set; }
        public virtual DbSet<OutsourcReport> OutsourcReports { get; set; }
    }
}
