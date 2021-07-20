using Infrastructure;
using Microsoft.EntityFrameworkCore;
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
        }
        public virtual DbSet<base_user> BaseUsers { get; set; }
        public virtual DbSet<wfa_eshop_canceledstatus> WfaEshopCanceledstatuses { get; set; }
        public virtual DbSet<wfa_eshop_oqutdetail> WfaEshopOqutdetails { get; set; }
        public virtual DbSet<wfa_eshop_status> WfaEshopStatuses { get; set; }
        public virtual DbSet<sbo_user> SboUsers { get; set; }
        public virtual DbSet<base_user_detail> BaseUserDetails { get; set; }
        public virtual DbSet<base_user_log> BaseUserLog { get; set; }


    }
}
