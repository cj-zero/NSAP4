using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.Repository
{
    /// <summary>
    /// 
    /// </summary>
    [ConnectionString("Nsap4HrDbContext")]
    public partial class Nsap4HrDbContext : DbContext
    {
        public Nsap4HrDbContext(DbContextOptions<Nsap4HrDbContext> options)
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

        public virtual DbSet<classroom_teacher> Classroom_Teachers { get; set; }
    }
}
