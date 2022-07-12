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

	[ConnectionString("NsapOaDbContext", DbType = "MySql")]
	public  class NsapOaDbContext : DbContext {
		public NsapOaDbContext(DbContextOptions<NsapOaDbContext> options) : base(options) 
		{
		}

		public static readonly LoggerFactory loggerFactory = new LoggerFactory(new[] { new DebugLoggerProvider() });
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			base.OnConfiguring(optionsBuilder);
			optionsBuilder.UseLoggerFactory(loggerFactory);
		}
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<file_main>().HasKey(s => new { s.file_id });
			modelBuilder.Entity<file_type>().HasKey(s => new { s.type_id });
		}
		public virtual DbSet<file_main> file_mains { get; set; }
		public virtual DbSet<file_type> file_types { get; set; }
	}
}
