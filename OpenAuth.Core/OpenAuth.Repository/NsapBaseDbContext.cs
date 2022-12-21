using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.NsapBase;
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
        public static readonly LoggerFactory loggerFactory = new LoggerFactory(new[] { new DebugLoggerProvider() });
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseLoggerFactory(loggerFactory);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //当主键为联合主键时，需要把这里的内容拷贝到对应的位置
            modelBuilder.Entity<wfa_eshop_status>().HasKey(o => o.document_id);
            modelBuilder.Entity<sbo_user>().HasKey(o => new { o.sbo_id, o.user_id });
            modelBuilder.Entity<base_user>().HasKey(o => o.user_id);
            modelBuilder.Entity<wfa_eshop_oqutdetail>().HasOne(s => s.wfa_Eshop_Status).WithMany(s => s.wfa_eshop_oqutdetails).HasForeignKey(s => s.document_id);
            modelBuilder.Entity<wfa_eshop_canceledstatus>().HasOne(s => s.wfa_Eshop_Status).WithMany(s => s.wfa_eshop_canceledstatuss).HasForeignKey(s => s.document_id);
            modelBuilder.Entity<base_user_detail>().HasKey(o => o.user_id);
            modelBuilder.Entity<base_user_log>().HasKey(o => o.Id);
            modelBuilder.Entity<WfaJobPara>().HasKey(o => new { o.job_id, o.para_idx });
            modelBuilder.Entity<wfa_job>().HasKey(o => o.job_id);
            modelBuilder.Entity<wfa_obj>().HasKey(o => o.step_id);
            modelBuilder.Entity<wfa_jump>().HasKey(o => o.job_id);
            modelBuilder.Entity<wfa_type>().HasKey(o => o.job_type_id);
            modelBuilder.Entity<wfa_step>().HasKey(o => o.step_id);
            modelBuilder.Entity<base_dep>().HasKey(o => o.dep_id);
            modelBuilder.Entity<base_contact>().HasKey(o => o.seq_id);
            modelBuilder.Entity<sbo_info>().HasKey(o => o.sbo_id);
            modelBuilder.Entity<base_user_role>().HasKey(o => new { o.role_id, o.user_id });
        }

        public virtual DbSet<sbo_info> sbo_infos { get; set; }
        public virtual DbSet<base_user> BaseUsers { get; set; }
        public virtual DbSet<wfa_eshop_canceledstatus> WfaEshopCanceledstatuses { get; set; }
        public virtual DbSet<wfa_eshop_oqutdetail> WfaEshopOqutdetails { get; set; }
        public virtual DbSet<wfa_eshop_status> WfaEshopStatuses { get; set; }
        public virtual DbSet<sbo_user> SboUsers { get; set; }
        public virtual DbSet<base_user_role> BaseUserRole { get; set; }
        public virtual DbSet<base_user_detail> BaseUserDetails { get; set; }
        public virtual DbSet<base_user_log> BaseUserLog { get; set; }
        public virtual DbSet<WfaJobPara> WfaJobPara { get; set; }
        public virtual DbSet<wfa_job> wfa_job { get; set; }
        public virtual DbSet<wfa_obj> wfa_obj { get; set; }
        public virtual DbSet<wfa_jump> wfa_junp { get; set; }
        public virtual DbSet<wfa_type> wfa_type { get; set; }
        public virtual DbSet<wfa_step> wfa_step { get; set; }
        public virtual DbSet<base_dep> base_dep { get; set; }
        public virtual DbSet<base_contact> base_contact { get; set; }
    }
}
